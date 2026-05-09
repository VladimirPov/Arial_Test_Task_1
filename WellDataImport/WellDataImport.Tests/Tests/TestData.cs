using WellDataImport.Models;

namespace WellDataImport.Tests
{
    public static class TestData
    {
        public static List<Holes> GetSampleHoles() => new()
        {
            new Holes { Name = "BH-001", X = 500000.5, Y = 6000000.3, Z = 250.0, Length = 150.5, Level = 10.0, IssueDate = 20240115 },
            new Holes { Name = "BH-002", X = 500100.2, Y = 6000100.7, Z = 248.5, Length = 200.0, Level = 12.5, IssueDate = 20240116 },
            new Holes { Name = "BH-003", X = 500200.8, Y = 6000200.1, Z = 249.8, Length = 175.3, Level = 11.2, IssueDate = 20240117 }
        };

        public static List<Assay> GetSampleAssays() => new()
        {
            new Assay { Name = "BH-001", From = 0.0, To = 1.0, Au = 2.5 },
            new Assay { Name = "BH-001", From = 1.0, To = 2.0, Au = 3.1 },
            new Assay { Name = "BH-001", From = 2.0, To = 3.0, Au = 1.8 },
            new Assay { Name = "BH-002", From = 0.0, To = 1.5, Au = 4.2 },
            new Assay { Name = "BH-003", From = 0.0, To = 2.0, Au = 2.9 }
        };
    }
}
