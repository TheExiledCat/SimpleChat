using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleChat;

public class ProgramConfig
{
    static ReadOnlyDictionary<string, string> Config { get; private set; } = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
    static ReadOnlyDictionary<string, string DefaultConfig = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
    public static string Get(string key)
    {
        if (Config.ContainsKey(key))
            return Config[key];
        else if (DefaultConfig.ContainsKey(key))
        {
            return DefaultConfig[key];
        }
        else
        {
            throw new KeyNotFoundException();
        }
    }
    static string configPath = "%HOME%/.config/simplechat/config.json";

    public static void Load()
    {

        try
        {
            // Ensure the parent directory exists
            string destinationDir = Path.GetDirectoryName(Environment.ExpandEnvironmentVariables(configPath));
            if (destinationDir != null)
            {
                Directory.CreateDirectory(destinationDir);

            }
            string desinationFile = Environment.ExpandEnvironmentVariables(configPath);

            if (!File.Exists(desinationFile))
            {
                File.Copy("assets/defaultConfig.json", desinationFile);
            }
            StreamReader reader = File.OpenText(desinationFile);
            Config = new ReadOnlyDictionary<string, string>(JsonSerializer.Deserialize<Dictionary<string, string>>(reader.ReadToEnd()));

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error copying file: {ex.Message}");
        }



    }
}
