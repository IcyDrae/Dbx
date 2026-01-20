using Dbx.Data;
using Dbx.Output;
using MySql.Data.MySqlClient;
using System.Text;

namespace Dbx.Database
{
    public class MySqlProvider : IDatabaseProvider
    {
        private readonly DbConnection Connection;
        private MySqlConnection? MySqlConnection;

        public MySqlProvider(DbConnection Connection)
        {
            this.Connection = Connection;
        }

        public void Connect()
        {
            string ConnectionString =
                $"Server={this.Connection.Host};" +
                $"Port={this.Connection.Port};" +
                $"Database={Connection.Database};" +
                $"User={Connection.Username};" +
                $"Password={Connection.Password};";

            this.MySqlConnection = new MySqlConnection(ConnectionString);
            this.MySqlConnection.Open();
        }

        public List<string> ListTables()
        {
            if (this.MySqlConnection == null || this.MySqlConnection.State != System.Data.ConnectionState.Open)
            {
                this.Connect();
            }

            List<string> Tables = new List<string>();

            MySqlCommand Command = new MySqlCommand("SHOW TABLES;", this.MySqlConnection);
            MySqlDataReader Reader = Command.ExecuteReader();

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

            MySqlCommand Command = new MySqlCommand(Sql, this.MySqlConnection);
            var Result = Command.ExecuteScalar();

            this.MySqlConnection?.Close();

            return Result?.ToString() ?? "No result.";
        }

        public List<TableColumn> DescribeTable(string Name)
        {
            this.Connect();
            List<TableColumn> TableColumn = new List<TableColumn>();

            MySqlCommand command = new MySqlCommand($"DESCRIBE {Name};", this.MySqlConnection);
            using var Reader = command.ExecuteReader();

            while (Reader.Read())
            {
                string Key = Reader["Key"].ToString() ?? "";
                
                if (Key == "MUL")
                {
                    Key = "MUL -> FK";
                }

                TableColumn.Add(new TableColumn
                {
                    Name = Reader["Field"]?.ToString() ?? "",
                    Type = Reader["Type"]?.ToString() ?? "",
                    Nullable = Reader["Null"]?.ToString() ?? "",
                    Key = Key,
                    Default = Reader["Default"]?.ToString() ?? ""
                });
            }

            return TableColumn;
        }

        public List<string> ListRows(string TableName, int Page = 1, int PageSize = 10, string? WhereClause = null)
        {
            if (this.MySqlConnection == null || this.MySqlConnection.State != System.Data.ConnectionState.Open)
            {
                this.Connect();
            }

            List<string> Rows = new List<string>();

            int Offset = (Page - 1) * PageSize;
            
            string Sql = $"SELECT * FROM `{TableName}`";

            if (!string.IsNullOrWhiteSpace(WhereClause))
            {
                Sql += " WHERE " + WhereClause;
            }

            Sql += " LIMIT @Limit OFFSET @Offset;";

            using MySqlCommand Command = new MySqlCommand(Sql, this.MySqlConnection);
            Command.Parameters.AddWithValue("@Limit", PageSize);
            Command.Parameters.AddWithValue("@Offset", Offset);

            using MySqlDataReader Reader = Command.ExecuteReader();

            while (Reader.Read())
            {
                List<string> RowValues = new List<string>();
                for (int i = 0; i < Reader.FieldCount; i++)
                {
                    RowValues.Add(Reader[i]?.ToString() ?? "");
                }
                Rows.Add(string.Join("  |  ", RowValues));
            }

            return Rows;
        }

        public List<Dictionary<string, string>> Query(string Query)
        {
            if (this.MySqlConnection == null || this.MySqlConnection.State != System.Data.ConnectionState.Open)
            {
                this.Connect();
            }

            var Rows = new List<Dictionary<string, string>>();

            using MySqlCommand Command = new MySqlCommand(Query, this.MySqlConnection);

            if (Query.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                using MySqlDataReader Reader = Command.ExecuteReader();
                while (Reader.Read())
                {
                    var row = new Dictionary<string, string>();

                    for (int i = 0; i < Reader.FieldCount; i++)
                    {
                        string columnName = Reader.GetName(i);
                        string value = Reader[i]?.ToString() ?? "";

                        row[columnName] = value;
                    }

                    Rows.Add(row);
                }
            }
            else
            {
                int affected = Command.ExecuteNonQuery();

                Rows.Add(new Dictionary<string, string>
                {
                    { "result", $"Query executed successfully. {affected} rows affected." }
                });
            }

            return Rows;
        }
    }
}

