using Dbx.Data;
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
    }
}

