using System.IO;
using System.Text.Json;
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

                var config = new Configuration
                {
                    DbName = "",
                    Host = "",
                    Password = ""
                };

                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(FullConfigPath, json);
            }
        }
    }
}

