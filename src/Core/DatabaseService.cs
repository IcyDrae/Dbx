using Dbx.Database;
using Dbx.Data;

namespace Dbx.Core
{
    public class DatabaseService
    {
        private readonly IDatabaseProvider Provider;
        private readonly string ConnectionName;

        public DatabaseService(Configuration Configuration, string? ConnectionName = null)
        {
            this.ConnectionName = ConnectionName ?? Configuration.DefaultConnection;

            if (!Configuration.Connections.ContainsKey(this.ConnectionName))
            {
                throw new Exception($"Connection '{this.ConnectionName}' not found in configuration.");
            }

            var Connection = Configuration.Connections[this.ConnectionName];

            this.Provider = Connection.Type switch
            {
                "mysql" => new MySqlProvider(Connection),
                "postgres" => new PostgresProvider(Connection),
                _ => throw new Exception("Unsupported database type.")
            };
        }

        public void Connect()
        {
            this.Provider.Connect();
        }

        public string RunQuery(string Sql)
        {
            return this.Provider.RunQuery(Sql);
        }

        public List<string> ListTables()
        {
            return this.Provider.ListTables();
        }

        public string GetConnectionName() => this.ConnectionName;
    }
}

