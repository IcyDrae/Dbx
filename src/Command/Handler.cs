using Dbx.Filesystem;
using Dbx.Data;
using Dbx.Core;
using Dbx.Output;

namespace Dbx
{
    public class Handler
    {
        ConfigurationFile ConfigurationFile;

        public Handler()
        {
            this.ConfigurationFile = new ConfigurationFile();
        }

        public void Execute(string[] arguments)
        {
            ParseArguments(arguments);
        }

        private void ParseArguments(string[] args)
        {
            var actualArgs = args;

            actualArgs = HandleDLLArguments(actualArgs);
            HandleNoArguments(actualArgs);

            if (actualArgs.Length == 0)
            {
                return;
            }

            HandleCommand(actualArgs[0], actualArgs[1..]);
        }

        private string[] HandleDLLArguments(string[] args)
        {
            var actualArgs = args;

            if (args.Length > 0 && args[0].EndsWith(".dll"))
            {
                actualArgs = args.Skip(1).ToArray();
            }

            return actualArgs;
        }

        private void HandleNoArguments(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No command provided");
            }
        }

        public void HandleCommand(string command, string[] parameters)
        {
            switch (command.ToLower())
            {
                case "connect":
                    HandleConnect(parameters);
                    break;
                case "config":
                    HandleConfig(parameters);
                    break;
                case "use":
                    HandleUse(parameters);
                    break;
                case "tables":
                    HandleShowTables();
                    break;
                case "describe":
                    HandleDescribe(parameters);
                    break;
                case "list":
                    HandleListRows(parameters);
                    break;
                case "query":
                    HandleQuery(parameters);
                    break;
                case "help":
                    HandleHelp();
                    break;
                default:
                    Console.WriteLine($"Unknown command: {command}");
                    break;
            }
        }

        private void HandleConnect(string[] parameters)
        {
            if (parameters.Length == 0)
            {
                Console.WriteLine("Usage: dbx connect <connection-name>");
                return;
            }

            string ConnectionName = parameters[0];

            this.ConfigurationFile.Create();

            var connection = this.ConfigurationFile.GetConnection(ConnectionName);
            if (connection == null)
            {
                Console.WriteLine($"Connection '{ConnectionName}' not found in config.");
                return;
            }

            Console.WriteLine($"Connecting to {connection.Type} database '{connection.Database}' on {connection.Host}:{connection.Port} as {connection.Username}...");

            var Configuration = this.ConfigurationFile.LoadConfiguration();

            try
            {
                DatabaseService DatabaseService = new DatabaseService(Configuration, ConnectionName);

                Console.WriteLine($"Connecting to database using connection '{DatabaseService.GetConnectionName()}'...");

                var Tables = DatabaseService.ListTables();

                Console.WriteLine("Tables:");

                foreach (var Table in Tables)
                {
                    Console.WriteLine($" - {Table}");
                }
            }
            catch (Exception Exception)
            {
                Console.WriteLine("Connection failed:");
                Console.WriteLine(Exception.Message);
            }
        }

        private void HandleConfig(string[] parameters)
        {
            this.ConfigurationFile.Create();

            if (parameters.Length == 0)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("  dbx config add <name> <type> <host> <port> <db> <user> <pass>");
                Console.WriteLine("  dbx config list");
                return;
            }

            switch (parameters[0].ToLower())
            {
                case "add":
                    HandleConfigAdd(parameters[1..]);
                    break;
                case "list":
                    HandleConfigList();
                    break;
                default:
                    Console.WriteLine("Unknown config command.");
                    break;
            }
        }

        private void HandleConfigAdd(string[] parameters)
        {
            if (parameters.Length < 7)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("  dbx config add <name> <type> <host> <port> <db> <user> <pass>");
                return;
            }

            string name = parameters[0];
            string type = parameters[1];
            string host = parameters[2];
            int port = int.Parse(parameters[3]);
            string database = parameters[4];
            string username = parameters[5];
            string password = parameters[6];

            this.ConfigurationFile.SaveConnection(name, new DbConnection
            {
                Type = type,
                Host = host,
                Port = port,
                Database = database,
                Username = username,
                Password = password
            });

            Console.WriteLine($"Connection '{name}' saved successfully.");
        }

        private void HandleConfigList()
        {
            string? Connections = this.ConfigurationFile.ListConnections();

            if (string.IsNullOrWhiteSpace(Connections))
            {
                Console.WriteLine("No available connections.");
                return;
            }

            Console.WriteLine("Available connections:");
            Console.WriteLine(Connections);
        }

        private void HandleUse(string[] parameters)
        {
            if (parameters.Length == 0)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("  dbx use <connection-name>");

                return;
            }

            string ConnectionName = parameters[0];

            try
            {
                this.ConfigurationFile.SetDefaultConnection(ConnectionName);
                Console.WriteLine($"Using {ConnectionName} as current database...");
            }
            catch (Exception Ex) {
                Console.WriteLine($"Error: {Ex.Message}");
            }
        }

        private void HandleShowTables()
        {
            Configuration Config = this.ConfigurationFile.LoadConfiguration();
            string DefaultConnectionName = Config.DefaultConnection;

            List<string> Tables = new DatabaseService(Config, DefaultConnectionName).ListTables();

            Console.WriteLine($"Showing tables for database {DefaultConnectionName}");

            foreach (string Table in Tables)
            {
                Console.WriteLine($"  {Table}");
            }
        }

        private void HandleDescribe(string[] parameters)
        {
            if (parameters.Length == 0)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("  dbx describe <table-name>");

                return;
            }

            Configuration Config = this.ConfigurationFile.LoadConfiguration();
            string DefaultConnectionName = Config.DefaultConnection;
            string TableName = parameters[0];

            Console.WriteLine($"Describing {TableName} table from connection {DefaultConnectionName} ...");

            try
            {
                DatabaseService DatabaseService = new DatabaseService(Config, DefaultConnectionName);
                var Columns = DatabaseService.DescribeTable(TableName);
                Formatter Formatter = new Formatter();

                Console.WriteLine(Formatter.FormatTable(Columns, TableName));
            }
            catch (Exception Exception)
            {
                Console.WriteLine("Connection failed:");
                Console.WriteLine(Exception.Message);
            }
        }

        private void HandleListRows(string[] parameters)
        {
            if (parameters.Length == 0)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("  dbx list <table-name>");
                Console.WriteLine("  dbx list <table-name> --where \"first_name=userName\"");
                return;
            }

            string tableName = parameters[0];
            string WhereClause = "";

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] == "--where" && i + 1 < parameters.Length)
                {
                    WhereClause = parameters[i + 1];
                    i++;
                }
                else if (parameters[i].StartsWith("--where="))
                {
                    WhereClause = parameters[i].Substring("--where=".Length);
                }
            }

            try
            {
                Configuration config = this.ConfigurationFile.LoadConfiguration();
                string defaultConnectionName = config.DefaultConnection;
                DatabaseService databaseService = new DatabaseService(config, defaultConnectionName);

                int pageSize = 10;
                int page = 1; // PostgreSQL ListRows uses 1-based pages
                bool running = true;

                while (running)
                {
                    Console.Clear();
                    List<string> rows = databaseService.ListRows(tableName, page, pageSize, WhereClause);

                    if (rows.Count == 0)
                    {
                        Console.WriteLine("No rows found.");
                        running = false;
                        break;
                    }

                    Console.WriteLine($"Showing page {page} (first {rows.Count} rows) from table {tableName}\n");

                    foreach (var row in rows)
                    {
                        Console.WriteLine(row);
                    }

                    Console.WriteLine($"\n[n = next | p = previous | q = quit]");
                    ConsoleKeyInfo key = Console.ReadKey(true);

                    switch (key.Key)
                    {
                        case ConsoleKey.N:
                            if (rows.Count == pageSize) page++; // only advance if the page was full
                            break;
                        case ConsoleKey.P:
                            if (page > 1) page--;
                            break;
                        case ConsoleKey.Q:
                            running = false;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing rows: {ex.Message}");
            }
        }

        private void HandleQuery(string[] parameters)
        {
            if (parameters.Length == 0)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("  dbx query \"SELECT * FROM users\"");

                return;
            }

            Configuration Config = this.ConfigurationFile.LoadConfiguration();
            string DefaultConnectionName = Config.DefaultConnection;
            string Query = parameters[0];

            try
            {
                DatabaseService DatabaseService = new DatabaseService(Config, DefaultConnectionName);
                List<string> Result = DatabaseService.Query(Query);

                if (Result.Count == 0)
                {
                    Console.WriteLine("Query executed successfully. No rows returned.");
                }
                else
                {
                    foreach (string Row in Result)
                    {
                        Console.WriteLine(Row);
                    }
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine($"There was an error: {Ex.Message}");
            }

            Console.WriteLine();
        }

        private void HandleHelp()
        {
            Console.WriteLine("Dbx - Commands:");
            Console.WriteLine();
            Console.WriteLine("config");
            Console.WriteLine("  Initialize the configuration file (creates ~/dbx-config/config.json if missing).");
            Console.WriteLine();
            Console.WriteLine("config add <connection-name> <database-type> <host> <port> <database> <user-name> <password>");
            Console.WriteLine("  Add a connection with the parameters. Please use the parameters in order as described here.");
            Console.WriteLine();
            Console.WriteLine("config list");
            Console.WriteLine("  List available connections in the configuration file.");
            Console.WriteLine();
            Console.WriteLine("connect <connection-name>");
            Console.WriteLine("  Connect to a named database from your config.");
            Console.WriteLine();
            Console.WriteLine("help");
            Console.WriteLine("  Show this help.");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  dbx config");
            Console.WriteLine("  dbx connect local-postgres");
        }
    }
}


