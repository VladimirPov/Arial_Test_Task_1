using System.Data;
using System.Data.SQLite;
using System.IO;
using WellDataImport.Models;

namespace WellDataImport.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly string _pathToDB;

        public DatabaseService(string databasePath)
        {
            _pathToDB = databasePath;
            _connectionString = $"Data Source={databasePath};Version=3;";
        }

        public List<Holes> GetAllHoles()
        {
            var holes = new List<Holes>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT name, x, y, z, lenght, _level, issue_date FROM holes";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var hole = new Holes
                        {
                            Name = reader["name"].ToString(),
                            X = Convert.ToDouble(reader["x"]),
                            Y = Convert.ToDouble(reader["y"]),
                            Z = Convert.ToDouble(reader["z"]),
                            Length = Convert.ToDouble(reader["lenght"]),
                            Level = Convert.ToDouble(reader["_level"]),
                            IssueDate = Convert.ToDouble(reader["issue_date"]),
                        };
                        holes.Add(hole);
                    }
                }
            }
            return holes;
        }

        public List<Assay> GetAllAssay()
        {
            var assays = new List<Assay>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT holes.name as name, _from, _to, Au FROM assay, holes where assay.hole_id = holes.id";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var assay = new Assay
                        {
                            Name = reader["name"].ToString(),
                            From = Convert.ToDouble(reader["_from"]),
                            To = Convert.ToDouble(reader["_to"]),
                            Au = Convert.ToDouble(reader["Au"]),
                        };
                        assays.Add(assay);
                    }
                }
            }
            return assays;
        }

        public int InsertHoles(List<Holes> holes)
        {
            int insertedCount = 0;

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var hole in holes)
                    {
                        // Проверяем, существует ли уже такая запись
                        string checkQuery = "SELECT COUNT(*) FROM holes WHERE name = @name";
                        using (var checkCmd = new SQLiteCommand(checkQuery, connection))
                        {
                            checkCmd.Parameters.AddWithValue("@name", hole.Name);

                            long exists = (long)checkCmd.ExecuteScalar();

                            if (exists == 0)
                            {
                                // Вставляем новую запись
                                string insertQuery = @"INSERT INTO holes (name, x, y, z, lenght, _level, issue_date) 
                                                     VALUES (@name, @x, @y, @z, @lenght, @_level, @issue_date)";

                                using (var insertCmd = new SQLiteCommand(insertQuery, connection))
                                {
                                    insertCmd.Parameters.AddWithValue("@name", hole.Name);
                                    insertCmd.Parameters.AddWithValue("@x", hole.X);
                                    insertCmd.Parameters.AddWithValue("@y", hole.Y);
                                    insertCmd.Parameters.AddWithValue("@z", hole.Z);
                                    insertCmd.Parameters.AddWithValue("@lenght", hole.Length);
                                    insertCmd.Parameters.AddWithValue("@_level", hole.Level);
                                    insertCmd.Parameters.AddWithValue("@issue_date", hole.IssueDate);

                                    insertCmd.ExecuteNonQuery();
                                    insertedCount++;
                                }
                            }
                        }
                    }

                    transaction.Commit();
                }
            }
            return insertedCount;
        }

        public int InsertAssay(List<Assay> assays)
        {
            int insertedCount = 0;

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var assay in assays)
                    {
                        var holeId = GetHoleId(assay.Name, connection);
                        if (holeId < 0)
                            return 0;

                        // Проверяем, существует ли уже такая запись
                        string checkQuery = "SELECT COUNT(*) FROM assay WHERE hole_id = @holeId AND _from = @from AND _to = @to";
                        using (var checkCmd = new SQLiteCommand(checkQuery, connection))
                        {
                            checkCmd.Parameters.AddWithValue("@holeId", holeId);
                            checkCmd.Parameters.AddWithValue("@from", assay.From);
                            checkCmd.Parameters.AddWithValue("@to", assay.To);

                            long exists = (long)checkCmd.ExecuteScalar();

                            if (exists == 0)
                            {
                                // Вставляем новую запись
                                string insertQuery = @"INSERT INTO assay (hole_id, _from, _to, Au) 
                                                     VALUES (@holeId, @from, @to, @au)";

                                using (var insertCmd = new SQLiteCommand(insertQuery, connection))
                                {
                                    insertCmd.Parameters.AddWithValue("@holeId", holeId);
                                    insertCmd.Parameters.AddWithValue("@from", assay.From);
                                    insertCmd.Parameters.AddWithValue("@to", assay.To);
                                    insertCmd.Parameters.AddWithValue("@au", assay.Au);

                                    insertCmd.ExecuteNonQuery();
                                    insertedCount++;
                                }
                            }
                        }
                    }

                    transaction.Commit();
                }
            }
            return insertedCount;
        }

        // Проверка соединения с БД
        public bool TestConnection(out string error)
        {
            error = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(_pathToDB) || !Path.Exists(_pathToDB))
                {
                    throw new Exception("Не найден файл базы данных");
                }

                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private Int64 GetHoleId(string name, SQLiteConnection connection)
        {
            // Проверяем, существует ли уже такая запись
            string checkQuery = "SELECT id FROM holes WHERE name = @name";
            using (var checkCmd = new SQLiteCommand(checkQuery, connection))
            {
                checkCmd.Parameters.AddWithValue("@name", name);

                var data = checkCmd.ExecuteReader();
                DataTable table = new();
                table.Load(data);
                if (table.Rows.Count > 0)
                {
                    return table.Rows[0].Field<Int64>(0);
                }
            }
            return -1;
        }
    }
}