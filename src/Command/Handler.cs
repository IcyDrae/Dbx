using Dbx.Filesystem;

namespace Dbx
{
    public class Handler
    {
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
                    HandleConfig();
                    break;
                default:
                    Console.WriteLine($"Unknown command: {command}");
                    break;
            }
        }

        private void HandleConnect(string[] parameters)
        {
            
        }

        private void HandleConfig()
        {
            ConfigurationFile Config = new ConfigurationFile();
            Config.Create();
        }
    }
}


