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
    }
}

