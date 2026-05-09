using FluentAssertions;
using System.Data.SQLite;
using WellDataImport.Models;
using WellDataImport.Services;

namespace WellDataImport.Tests
{
    public class DatabaseServiceTests : IDisposable
    {
        private readonly string _testDbPath;
        private DatabaseService _databaseService;
        private readonly string _schemaScript = @"
            CREATE TABLE IF NOT EXISTS holes (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL UNIQUE,
                x REAL,
                y REAL,
                z REAL,
                lenght REAL,
                _level REAL,
                issue_date INTEGER
            );

            CREATE TABLE IF NOT EXISTS assay (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                hole_id INTEGER NOT NULL,
                _from REAL NOT NULL,
                _to REAL NOT NULL,
                Au REAL,
                FOREIGN KEY (hole_id) REFERENCES holes(id)
            );
        ";

        public DatabaseServiceTests()
        {
            _testDbPath = Path.Combine(Path.GetTempPath(), $"test_db_{Guid.NewGuid()}.db");
            InitializeDatabase();
            _databaseService = new DatabaseService(_testDbPath);
        }

        private void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection($"Data Source={_testDbPath};Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(_schemaScript, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Dispose()
        {
            _databaseService = null;
            if (File.Exists(_testDbPath))
                File.Delete(_testDbPath);
        }

        #region InsertHoles Tests

        [Fact]
        public void InsertHoles_WithValidData_ShouldInsertSuccessfully()
        {
            // Arrange
            var holes = new List<Holes>
            {
                new Holes { Name = "Hole-1", X = 100, Y = 200, Z = 50, Length = 120, Level = 10, IssueDate = 20240101 },
                new Holes { Name = "Hole-2", X = 150, Y = 250, Z = 75, Length = 180, Level = 15, IssueDate = 20240102 }
            };

            // Act
            var insertedCount = _databaseService.InsertHoles(holes);

            // Assert
            insertedCount.Should().Be(2);

            var allHoles = _databaseService.GetAllHoles();
            allHoles.Should().HaveCount(2);
            allHoles.Select(h => h.Name).Should().Contain(new[] { "Hole-1", "Hole-2" });
        }

        [Fact]
        public void InsertHoles_WhenDuplicateNameExists_ShouldNotInsertDuplicate()
        {
            // Arrange
            var holes = new List<Holes>
            {
                new Holes { Name = "Hole-1", X = 100, Y = 200, Z = 50, Length = 120, Level = 10, IssueDate = 20240101 }
            };
            _databaseService.InsertHoles(holes);

            var duplicateHole = new List<Holes>
            {
                new Holes { Name = "Hole-1", X = 999, Y = 999, Z = 999, Length = 999, Level = 999, IssueDate = 99999999 }
            };

            // Act
            var insertedCount = _databaseService.InsertHoles(duplicateHole);

            // Assert
            insertedCount.Should().Be(0);

            var allHoles = _databaseService.GetAllHoles();
            allHoles.Should().HaveCount(1);
            allHoles[0].X.Should().Be(100); // Original value preserved
        }

        [Fact]
        public void InsertHoles_WithEmptyList_ShouldReturnZero()
        {
            // Act
            var insertedCount = _databaseService.InsertHoles(new List<Holes>());

            // Assert
            insertedCount.Should().Be(0);
        }

        #endregion

        #region InsertAssay Tests

        [Fact]
        public void InsertAssay_WithValidData_ShouldInsertSuccessfully()
        {
            // Arrange
            var holes = new List<Holes>
            {
                new Holes { Name = "Hole-1", X = 100, Y = 200, Z = 50, Length = 120, Level = 10, IssueDate = 20240101 }
            };
            _databaseService.InsertHoles(holes);

            var assays = new List<Assay>
            {
                new Assay { Name = "Hole-1", From = 0, To = 1.5, Au = 2.5 },
                new Assay { Name = "Hole-1", From = 1.5, To = 3.0, Au = 1.8 }
            };

            // Act
            var insertedCount = _databaseService.InsertAssay(assays);

            // Assert
            insertedCount.Should().Be(2);

            var allAssays = _databaseService.GetAllAssay();
            allAssays.Should().HaveCount(2);
            allAssays.Should().AllSatisfy(a => a.Name.Should().Be("Hole-1"));
        }

        [Fact]
        public void InsertAssay_WhenHoleNotFound_ShouldNotInsert()
        {
            // Arrange
            var assays = new List<Assay>
            {
                new Assay { Name = "NonExistentHole", From = 0, To = 1.5, Au = 2.5 }
            };

            // Act
            var insertedCount = _databaseService.InsertAssay(assays);

            // Assert
            insertedCount.Should().Be(0);

            var allAssays = _databaseService.GetAllAssay();
            allAssays.Should().BeEmpty();
        }

        [Fact]
        public void InsertAssay_WhenDuplicateExists_ShouldNotInsertDuplicate()
        {
            // Arrange
            var holes = new List<Holes>
            {
                new Holes { Name = "Hole-1", X = 100, Y = 200, Z = 50, Length = 120, Level = 10, IssueDate = 20240101 }
            };
            _databaseService.InsertHoles(holes);

            var assay = new List<Assay>
            {
                new Assay { Name = "Hole-1", From = 0, To = 1.5, Au = 2.5 }
            };
            _databaseService.InsertAssay(assay);

            var duplicateAssay = new List<Assay>
            {
                new Assay { Name = "Hole-1", From = 0, To = 1.5, Au = 999.9 }
            };

            // Act
            var insertedCount = _databaseService.InsertAssay(duplicateAssay);

            // Assert
            insertedCount.Should().Be(0);

            var allAssays = _databaseService.GetAllAssay();
            allAssays.Should().HaveCount(1);
            allAssays[0].Au.Should().Be(2.5); // Original value preserved
        }

        [Fact]
        public void InsertAssay_WithMultipleDifferentHoles_ShouldInsertCorrectly()
        {
            // Arrange
            var holes = new List<Holes>
            {
                new Holes { Name = "Hole-1", X = 100, Y = 200, Z = 50, Length = 120, Level = 10, IssueDate = 20240101 },
                new Holes { Name = "Hole-2", X = 150, Y = 250, Z = 75, Length = 180, Level = 15, IssueDate = 20240102 }
            };
            _databaseService.InsertHoles(holes);

            var assays = new List<Assay>
            {
                new Assay { Name = "Hole-1", From = 0, To = 1.5, Au = 2.5 },
                new Assay { Name = "Hole-2", From = 0, To = 2.0, Au = 3.2 }
            };

            // Act
            var insertedCount = _databaseService.InsertAssay(assays);

            // Assert
            insertedCount.Should().Be(2);

            var allAssays = _databaseService.GetAllAssay();
            allAssays.Should().HaveCount(2);
            allAssays.Select(a => a.Name).Should().Contain(new[] { "Hole-1", "Hole-2" });
        }

        #endregion

        #region GetAllHoles & GetAllAssay Tests

        [Fact]
        public void GetAllHoles_WhenDatabaseEmpty_ShouldReturnEmptyList()
        {
            // Act
            var holes = _databaseService.GetAllHoles();

            // Assert
            holes.Should().BeEmpty();
        }

        [Fact]
        public void GetAllHoles_WithExistingData_ShouldReturnAllHoles()
        {
            // Arrange
            var expectedHoles = new List<Holes>
            {
                new Holes { Name = "Hole-1", X = 100, Y = 200, Z = 50, Length = 120, Level = 10, IssueDate = 20240101 },
                new Holes { Name = "Hole-2", X = 150, Y = 250, Z = 75, Length = 180, Level = 15, IssueDate = 20240102 }
            };
            _databaseService.InsertHoles(expectedHoles);

            // Act
            var actualHoles = _databaseService.GetAllHoles();

            // Assert
            actualHoles.Should().BeEquivalentTo(expectedHoles);
        }

        [Fact]
        public void GetAllAssay_ShouldJoinWithHolesCorrectly()
        {
            // Arrange
            var holes = new List<Holes>
            {
                new Holes { Name = "Hole-1", X = 100, Y = 200, Z = 50, Length = 120, Level = 10, IssueDate = 20240101 }
            };
            _databaseService.InsertHoles(holes);

            var assays = new List<Assay>
            {
                new Assay { Name = "Hole-1", From = 0, To = 1.5, Au = 2.5 },
                new Assay { Name = "Hole-1", From = 1.5, To = 3.0, Au = 1.8 }
            };
            _databaseService.InsertAssay(assays);

            // Act
            var result = _databaseService.GetAllAssay();

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(a => a.Name.Should().Be("Hole-1"));
            result.Should().BeEquivalentTo(assays);
        }

        #endregion

        #region TestConnection Tests

        [Fact]
        public void TestConnection_WithValidDatabase_ShouldReturnTrue()
        {
            // Act
            bool result = _databaseService.TestConnection(out string error);

            // Assert
            result.Should().BeTrue();
            error.Should().BeEmpty();
        }

        [Fact]
        public void TestConnection_WhenDatabaseNotFound_ShouldReturnFalse()
        {
            // Arrange
            var invalidDbService = new DatabaseService("C:\\NonExistent\\Path\\database.db");

            // Act
            bool result = invalidDbService.TestConnection(out string error);

            // Assert
            result.Should().BeFalse();
            error.Should().NotBeEmpty();
        }

        #endregion
    }
}
