using Dbx.Data;
using Npgsql;

namespace Dbx.Database
{
    public class PostgresProvider : IDatabaseProvider
    {
        private readonly DbConnection DbConnection;
        private NpgsqlConnection? PostgresConnection;

        public PostgresProvider(DbConnection DbConnection)
        {
            this.DbConnection = DbConnection;
        }

        public void Connect()
        {
            string connString =
                $"Host={DbConnection.Host};" +
                $"Port={DbConnection.Port};" +
                $"Database={DbConnection.Database};" +
                $"Username={DbConnection.Username};" +
                $"Password={DbConnection.Password}";

            PostgresConnection = new NpgsqlConnection(connString);
            PostgresConnection.Open();
        }

        public List<string> ListTables()
        {
            if (this.PostgresConnection == null || this.PostgresConnection.State != System.Data.ConnectionState.Open)
            {
                this.Connect();
            }

            List<string> Tables = new List<string>();
            using var Command = new NpgsqlCommand(
                "SELECT tablename FROM pg_tables WHERE schemaname='public';",
                this.PostgresConnection
            );
            using var Reader = Command.ExecuteReader();
            
            while (Reader.Read())
            {
                Tables.Add(Reader.GetString(0));
            }

            return Tables;
        }

        public string RunQuery(string Sql)
        {
            if (this.PostgresConnection == null || this.PostgresConnection.State != System.Data.ConnectionState.Open)
            {
                Connect();
            }

            using var Command = new NpgsqlCommand(Sql, this.PostgresConnection);
            var Result = Command.ExecuteScalar();

            return Result?.ToString() ?? "No result.";
        }
    }
}

