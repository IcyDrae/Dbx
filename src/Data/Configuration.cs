
namespace Dbx.Data
{
    public class Configuration
    {
        public string DefaultConnection { get; set; }  = "";

        public Dictionary<string, DbConnection> Connections { get; set; } = new();
    }

    public class DbConnection
    {
        public string Type { get; set; } = "postgres"; // sqlite, mysql, sqlserver
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5432;
        public string Database { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = ""; // later: encrypted
    }
}

