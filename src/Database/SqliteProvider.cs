using Dbx.Data;
using Dbx.Output;
using Microsoft.Data.Sqlite;

namespace Dbx.Database
{
    public class SqliteProvider : IDatabaseProvider
    {
        private readonly DbConnection Connection;
        private SqliteConnection? SqliteConnection;

        public SqliteProvider(DbConnection Connection)
        {
            this.Connection = Connection;
        }

        public void Connect()
        {
            string ConnectionString =
                $"Data Source={this.Connection.Database};";

            this.SqliteConnection = new SqliteConnection(ConnectionString);
            this.SqliteConnection.Open();
        }

        public List<string> ListTables()
        {
            if (this.SqliteConnection == null || this.SqliteConnection.State != System.Data.ConnectionState.Open)
            {
                this.Connect();
            }

            List<string> Tables = new List<string>();

            SqliteCommand Command = new SqliteCommand("SELECT name FROM sqlite_master WHERE type='table';",
                                                      this.SqliteConnection);
            SqliteDataReader Reader = Command.ExecuteReader();

            while (Reader.Read())
            {
                Tables.Add(Reader.GetString(0));
            }

            Reader.Close();

            return Tables;
        }

        public string RunQuery(string Sql)
        {
            this.Connect();

            SqliteCommand Command = new SqliteCommand(Sql, this.SqliteConnection);
            var Result = Command.ExecuteScalar();

            this.SqliteConnection?.Close();

            return Result?.ToString() ?? "No result.";
        }

        public List<TableColumn> DescribeTable(string Name)
        {
            this.Connect();
            List<TableColumn> TableColumn = new List<TableColumn>();

            string Sql = $"PRAGMA table_info({Name})";

            SqliteCommand command = new SqliteCommand(Sql, this.SqliteConnection);
            using var Reader = command.ExecuteReader();

            while (Reader.Read())
            {
                TableColumn.Add(new TableColumn
                {
                    Name = Reader["name"]?.ToString() ?? "",
                    Type = Reader["type"]?.ToString() ?? "",
                    Nullable = (Convert.ToInt32(Reader["notnull"]) == 0) ? "YES" : "NO",
                    Key = (Convert.ToInt32(Reader["pk"]) == 1) ? "PRI" : "",
                    Default = Reader["dflt_value"]?.ToString() ?? ""
                });
            }

            return TableColumn;
        }

        public List<string> ListRows(string tableName, int page = 1, int pageSize = 10, string? WhereClause = null)
        {
            if (this.SqliteConnection == null || this.SqliteConnection.State != System.Data.ConnectionState.Open)
                this.Connect();

            var rows = new List<string>();
            int offset = (page - 1) * pageSize;
            
            string Sql = $"SELECT * FROM `{tableName}`";

            if (!string.IsNullOrWhiteSpace(WhereClause))
            {
                Sql += " WHERE " + WhereClause;
            }

            Sql += " LIMIT @Limit OFFSET @Offset;";

            using var cmd = new SqliteCommand(Sql, this.SqliteConnection);
            cmd.Parameters.AddWithValue("@Limit", pageSize);
            cmd.Parameters.AddWithValue("@Offset", offset);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var rowValues = new List<string>();
                for (int i = 0; i < reader.FieldCount; i++)
                    rowValues.Add(reader[i]?.ToString() ?? "");

                rows.Add(string.Join("  |  ", rowValues));
            }

            return rows;
        }

        public List<Dictionary<string, string>> Query(string Query)
        {
            if (this.SqliteConnection == null || this.SqliteConnection.State != System.Data.ConnectionState.Open)
            {
                this.Connect();
            }

            List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();

            using var command = new SqliteCommand(Query, this.SqliteConnection);
            using var reader = command.ExecuteReader();

            var columnNames = Enumerable.Range(0, reader.FieldCount)
                                .Select(i => reader.GetName(i))
                                .ToArray();

            while (reader.Read())
            {
                var row = new Dictionary<string, string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[columnNames[i]] = reader[i]?.ToString() ?? "";
                }
                result.Add(row);
            }

            return result;
        }
    }
}

