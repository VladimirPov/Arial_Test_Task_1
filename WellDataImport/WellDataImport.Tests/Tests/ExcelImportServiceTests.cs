using FluentAssertions;
using OfficeOpenXml;
using WellDataImport.Models;
using WellDataImport.Services;

namespace WellDataImport.Tests
{
    public class ExcelImportServiceTests : IDisposable
    {
        private readonly string _testFilePath;
        private readonly ExcelImportService _service;

        public ExcelImportServiceTests()
        {
            _service = new ExcelImportService();
            _testFilePath = Path.Combine(Path.GetTempPath(), $"test_well_data_{Guid.NewGuid()}.xlsx");
            ExcelPackage.License.SetNonCommercialPersonal("WellDataImport");
        }

        public void Dispose()
        {
            if (File.Exists(_testFilePath))
                File.Delete(_testFilePath);
        }

        #region ImportHolesFromExcel Tests

        [Fact]
        public void ImportHolesFromExcel_WithValidData_ShouldReturnCorrectHolesList()
        {
            // Arrange
            CreateTestExcelFile(hasHolesSheet: true, hasAssaySheet: false, holesData: new[]
            {
                new object[] { "Hole-1", 100.5, 200.3, 50.2, 120.0, 10.5, 20240101 },
                new object[] { "Hole-2", 150.7, 250.8, 75.1, 180.5, 15.2, 20240102 }
            });

            // Act
            var result = _service.ImportHolesFromExcel(_testFilePath);

            // Assert
            result.Should().HaveCount(2);
            result[0].Should().BeEquivalentTo(new Holes
            {
                Name = "Hole-1",
                X = 100.5,
                Y = 200.3,
                Z = 50.2,
                Length = 120.0,
                Level = 10.5,
                IssueDate = 20240101
            });
            result[1].Name.Should().Be("Hole-2");
            result[1].X.Should().Be(150.7);
        }

        [Fact]
        public void ImportHolesFromExcel_WhenHolesSheetMissing_ShouldThrowException()
        {
            // Arrange
            CreateTestExcelFile(hasHolesSheet: false, hasAssaySheet: false);

            // Act
            Action act = () => _service.ImportHolesFromExcel(_testFilePath);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("Отсутствует рабочий лист Holes");
        }

        [Fact]
        public void ImportHolesFromExcel_WhenWorksheetEmpty_ShouldReturnEmptyList()
        {
            // Arrange
            CreateTestExcelFile(hasHolesSheet: true, hasAssaySheet: false, holesData: null, isEmptyHoles: true);

            // Act
            var result = _service.ImportHolesFromExcel(_testFilePath);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void ImportHolesFromExcel_WhenNameIsEmpty_ShouldThrowException()
        {
            // Arrange
            CreateTestExcelFile(hasHolesSheet: true, hasAssaySheet: false, holesData: new[]
            {
                new object[] { "", 100.5, 200.3, 50.2, 120.0, 10.5, 20240101 }
            });

            // Act
            Action act = () => _service.ImportHolesFromExcel(_testFilePath);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("Не верный формат данных! name = *");
        }

        [Theory]
        [InlineData("invalid_number", 1)]
        [InlineData("abc", 2)]
        [InlineData("12.34.56", 3)]
        public void ImportHolesFromExcel_WhenInvalidNumberFormat_ShouldThrowException(
            string invalidValue, int columnIndex)
        {
            // Arrange
            var rowData = new object[] { "Hole-1", 100.5, 200.3, 50.2, 120.0, 10.5, 20240101 };
            rowData[columnIndex] = invalidValue;

            CreateTestExcelFile(hasHolesSheet: true, hasAssaySheet: false, holesData: new[] { rowData });

            // Act
            Action act = () => _service.ImportHolesFromExcel(_testFilePath);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage($"Не верный формат данных! {invalidValue}");
        }

        [Fact]
        public void ImportHolesFromExcel_WithDecimalPoint_ShouldParseCorrectly()
        {
            // Arrange
            CreateTestExcelFile(hasHolesSheet: true, hasAssaySheet: false, holesData: new[]
            {
                new object[] { "Hole-1", 100.5, 200.8, 50.9, "120.75", "10.25", 20240101 }
            });

            // Act
            var result = _service.ImportHolesFromExcel(_testFilePath);

            // Assert
            result[0].Length.Should().Be(120.75);
            result[0].Level.Should().Be(10.25);
        }

        #endregion

        #region ImportAssayFromExcel Tests

        [Fact]
        public void ImportAssayFromExcel_WithValidData_ShouldReturnCorrectAssayList()
        {
            // Arrange
            CreateTestExcelFile(hasHolesSheet: false, hasAssaySheet: true, assayData: new[]
            {
                new object[] { "Hole-1", 0.0, 1.5, 2.5 },
                new object[] { "Hole-1", 1.5, 3.0, 1.8 },
                new object[] { "Hole-2", 0.0, 2.0, 3.2 }
            });

            // Act
            var result = _service.ImportAssayFromExcel(_testFilePath);

            // Assert
            result.Should().HaveCount(3);
            result[0].Should().BeEquivalentTo(new Assay
            {
                Name = "Hole-1",
                From = 0.0,
                To = 1.5,
                Au = 2.5
            });
            result[1].Au.Should().Be(1.8);
            result[2].Name.Should().Be("Hole-2");
        }

        [Fact]
        public void ImportAssayFromExcel_WhenAssaySheetMissing_ShouldThrowException()
        {
            // Arrange
            CreateTestExcelFile(hasHolesSheet: false, hasAssaySheet: false);

            // Act
            Action act = () => _service.ImportAssayFromExcel(_testFilePath);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("Отсутствует рабочий лист Assay");
        }

        [Fact]
        public void ImportAssayFromExcel_WhenNameIsEmpty_ShouldThrowException()
        {
            // Arrange
            CreateTestExcelFile(hasHolesSheet: false, hasAssaySheet: true, assayData: new[]
            {
                new object[] { "", 0.0, 1.5, 2.5 }
            });

            // Act
            Action act = () => _service.ImportAssayFromExcel(_testFilePath);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("Не верный формат данных! name = *");
        }

        [Fact]
        public void ImportAssayFromExcel_WhenWorksheetEmpty_ShouldReturnEmptyList()
        {
            // Arrange
            CreateTestExcelFile(hasHolesSheet: false, hasAssaySheet: true, assayData: null, isEmptyAssay: true);

            // Act
            var result = _service.ImportAssayFromExcel(_testFilePath);

            // Assert
            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData("invalid", 1)]
        [InlineData("abc", 2)]
        [InlineData("xyz", 3)]
        public void ImportAssayFromExcel_WhenInvalidNumber_ShouldThrowException(
            string invalidValue, int columnIndex)
        {
            // Arrange
            var rowData = new object[] { "Hole-1", 0.0, 1.5, 2.5 };
            rowData[columnIndex] = invalidValue;

            CreateTestExcelFile(hasHolesSheet: false, hasAssaySheet: true, assayData: new[] { rowData });

            // Act
            Action act = () => _service.ImportAssayFromExcel(_testFilePath);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage($"Не верный формат данных! {invalidValue}");
        }

        #endregion

        #region Helper Methods

        private void CreateTestExcelFile(
            bool hasHolesSheet = true,
            bool hasAssaySheet = true,
            object[][] holesData = null,
            bool isEmptyHoles = false,
            object[][] assayData = null,
            bool isEmptyAssay = false)
        {
            using (var package = new ExcelPackage(new FileInfo(_testFilePath)))
            {
                package.Workbook.Worksheets.Add("Some Worksheet");
                if (hasHolesSheet)
                {
                    var holesSheet = package.Workbook.Worksheets.Add("Holes");

                    if (!isEmptyHoles && holesData != null && holesData.Any())
                    {
                        // Add headers
                        holesSheet.Cells[1, 1].Value = "Name";
                        holesSheet.Cells[1, 2].Value = "X";
                        holesSheet.Cells[1, 3].Value = "Y";
                        holesSheet.Cells[1, 4].Value = "Z";
                        holesSheet.Cells[1, 5].Value = "Length";
                        holesSheet.Cells[1, 6].Value = "Level";
                        holesSheet.Cells[1, 7].Value = "IssueDate";

                        // Add data
                        for (int i = 0; i < holesData.Length; i++)
                        {
                            for (int j = 0; j < holesData[i].Length; j++)
                            {
                                holesSheet.Cells[i + 2, j + 1].Value = holesData[i][j];
                            }
                        }
                    }
                }

                if (hasAssaySheet)
                {
                    var assaySheet = package.Workbook.Worksheets.Add("Assay");

                    if (!isEmptyAssay && assayData != null && assayData.Any())
                    {
                        // Add headers
                        assaySheet.Cells[1, 1].Value = "Name";
                        assaySheet.Cells[1, 2].Value = "From";
                        assaySheet.Cells[1, 3].Value = "To";
                        assaySheet.Cells[1, 4].Value = "Au";

                        // Add data
                        for (int i = 0; i < assayData.Length; i++)
                        {
                            for (int j = 0; j < assayData[i].Length; j++)
                            {
                                assaySheet.Cells[i + 2, j + 1].Value = assayData[i][j];
                            }
                        }
                    }
                }

                package.Save();
            }
        }

        #endregion
    }
}
