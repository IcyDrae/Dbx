using System.IO;
using System.Text.Json;
using System.Text;
using Dbx.Data;

namespace Dbx.Filesystem
{
    public class ConfigurationFile
    {
        public static string DefaultFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/dbx-config";
        public const string FileName = "config.json";

        public void Create()
        {
            if (!Directory.Exists(DefaultFolderPath))
            {
                Directory.CreateDirectory(DefaultFolderPath);
            }

            string FullConfigPath = Path.Combine(DefaultFolderPath, FileName);
            if (!File.Exists(FullConfigPath))
            {
                File.Create(FullConfigPath).Close();
                var Config = new Configuration();

                string json = JsonSerializer.Serialize(Config, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(FullConfigPath, json);
            }
        }

        public void SaveConnection(string Name, DbConnection Connection)
        {
            string Path = System.IO.Path.Combine(DefaultFolderPath, FileName);

            var Json = File.ReadAllText(Path);
            var Config = JsonSerializer.Deserialize<Configuration>(Json) ?? new Configuration();

            Config.Connections[Name] = Connection;
            Config.DefaultConnection = Name;

            string UpdatedJson = JsonSerializer.Serialize(Config,
                new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(Path, UpdatedJson);
        }

        public DbConnection? GetConnection(string Name)
        {
            string ConfigPath = System.IO.Path.Combine(DefaultFolderPath, FileName);

            if (!File.Exists(ConfigPath))
            {
                return null;
            }

            var Json = File.ReadAllText(ConfigPath);
            
            if (string.IsNullOrWhiteSpace(Json))
            {
                return null;
            }

            var Config = JsonSerializer.Deserialize<Configuration>(Json);
            
            if (Config == null || !Config.Connections.ContainsKey(Name))
            {
                return null;
            }

            return Config.Connections[Name];
        }

        public string? ListConnections()
        {
            string ConfigPath = System.IO.Path.Combine(DefaultFolderPath, FileName);

            if (!File.Exists(ConfigPath))
            {
                return null;
            }

            var Json = File.ReadAllText(ConfigPath);
            
            if (string.IsNullOrWhiteSpace(Json))
            {
                return null;
            }

            var Config = JsonSerializer.Deserialize<Configuration>(Json);

            if (Config == null)
            {
                return null;
            }

            StringBuilder Sb = new StringBuilder();

            foreach (var Connection in Config.Connections)
            {
                Sb.AppendLine(
                    $"{Connection.Key} -> {Connection.Value.Type} @ {Connection.Value.Host}:{Connection.Value.Port}/{Connection.Value.Database}"
                );
            }

            return Sb.ToString();
        }

        public Configuration LoadConfiguration()
        {
            string path = Path.Combine(DefaultFolderPath, FileName);

            if (!File.Exists(path))
            {
                throw new Exception("Configuration file does not exist.");
            }

            var Json = File.ReadAllText(path);
            var configuration = JsonSerializer.Deserialize<Configuration>(Json);

            if (configuration == null)
            {
                throw new Exception("Configuration file is empty or corrupted.");
            }

            return configuration;
        }

        public void SetDefaultConnection(string Name)
        {
            string path = Path.Combine(DefaultFolderPath, FileName);
            string Json = File.ReadAllText(path);
            var Config = JsonSerializer.Deserialize<Configuration>(Json) ?? new Configuration();

            if (!Config.Connections.ContainsKey(Name))
            {
                throw new Exception($"Connection {Name} does not exist.");
            }

            Config.DefaultConnection = Name;

            string UpdatedJson = JsonSerializer.Serialize(Config,
            new JsonSerializerOptions {
                WriteIndented = true
            });

            File.WriteAllText(path, UpdatedJson);
        }
    }
}

