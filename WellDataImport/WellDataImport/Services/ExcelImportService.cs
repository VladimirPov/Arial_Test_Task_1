using OfficeOpenXml;
using System.IO;
using WellDataImport.Models;

namespace WellDataImport.Services
{
    public class ExcelImportService
    {
        public List<Holes> ImportHolesFromExcel(string filePath)
        {
            var holesList = new List<Holes>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                if (!package.Workbook.Worksheets.Any((x) => x.Name == "Holes"))
                    throw new Exception("Отсутствует рабочий лист Holes");
                var worksheet = package.Workbook.Worksheets.First((x) => x.Name == "Holes");

                if (worksheet.Dimension == null)
                    return holesList;

                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    var name = GetCellValue(worksheet, row, 1);     
                    var x = ConvertToDouble(GetCellValue(worksheet, row, 2));    
                    var y = ConvertToDouble(GetCellValue(worksheet, row, 3));     
                    var z = ConvertToDouble(GetCellValue(worksheet, row, 4));
                    var length = ConvertToDouble(GetCellValue(worksheet, row, 5));
                    var level = ConvertToDouble(GetCellValue(worksheet, row, 6));
                    var issueDate = ConvertToDouble(GetCellValue(worksheet, row, 7));

                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        holesList.Add(new Holes
                        {
                            Name = name,
                            X = x,
                            Y = y,
                            Z = z,
                            Length = length,
                            Level = level,
                            IssueDate = issueDate,
                        });
                    }
                    else
                    {
                        throw new Exception($"Не верный формат данных! name = {name}");
                    }
                }
            }
            return holesList;
        }

        public List<Assay> ImportAssayFromExcel(string filePath)
        {
            var assayList = new List<Assay>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                if (!package.Workbook.Worksheets.Any((x) => x.Name == "Assay"))
                    throw new Exception("Отсутствует рабочий лист Assay");
                var worksheet = package.Workbook.Worksheets.First((x) => x.Name == "Assay");

                if (worksheet.Dimension == null)
                    return assayList;

                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    var name = GetCellValue(worksheet, row, 1);
                    var from = ConvertToDouble(GetCellValue(worksheet, row, 2));
                    var to = ConvertToDouble(GetCellValue(worksheet, row, 3));
                    var au = ConvertToDouble(GetCellValue(worksheet, row, 4));

                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        assayList.Add(new Assay
                        {
                            Name = name,
                            From = from,
                            To = to,
                            Au = au,
                        });
                    }
                    else
                    {
                        throw new Exception($"Не верный формат данных! name = {name}");
                    }
                }
            }
            return assayList;
        }

        private string GetCellValue(ExcelWorksheet worksheet, int row, int col)
        {
            var cell = worksheet.Cells[row, col];
            return cell.Value?.ToString()?.Trim() ?? string.Empty;
        }

        private double ConvertToDouble(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            // Замена точки на запятую для корректного парсинга
            string newValue = value.Replace('.', ',');

            if (double.TryParse(newValue, out double result)) 
            {
                return result;
            }
            else
            {
                throw new Exception($"Не верный формат данных! {value}");
            }
        }
    }
}
