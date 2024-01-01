using Meadow;
using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using YamlDotNet.Core;

namespace Clima_IoTHub.Model
{
    public class AppSettings
    {
        public int TestRunId { get; set; } = 0;
    }
    /// <summary>
    /// Class to force compiler to generate Json Serialisation code
    /// </summary>
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(AppSettings))]
    internal partial class MyJsonContext : JsonSerializerContext
    {
    }

    public class AppSettingsController
    {
        public AppSettings AppSettings { get; private set; } = new AppSettings();

        private string settingsFile { get; set; }
        private string settingsPath { get; set; }

        public AppSettingsController(string path = "", string filename= "settings.json")
        {
            settingsPath = path;
            settingsFile = Path.Combine(path, filename);
        }

        public AppSettings Read()
        {
            try
            {
                var result = File.Exists(settingsFile);
                if (!result)
                {
                    Resolver.Log.Info($"Creating '{settingsFile}'...");
                    Save();
                }

                string jsonText = File.ReadAllText(settingsFile);
                AppSettings = JsonSerializer.Deserialize(jsonText, MyJsonContext.Default.AppSettings);
            }
            catch (Exception e)
            { 
                Resolver.Log.Error($"Error: Message={e.Message}");
                AppSettings = new AppSettings();
            }
            return AppSettings;
        }

        public void Save()
        {
            Resolver.Log.Info($"Save '{settingsFile}'...");

            try
            {
                string jsonText = JsonSerializer.Serialize(AppSettings, MyJsonContext.Default.AppSettings);

                var result = Directory.Exists(settingsPath);
                if (!result)
                {
                    Resolver.Log.Info($"Directory doesn't exist, creating: '{settingsPath}'");
                    Directory.CreateDirectory(settingsPath);
                }

                File.WriteAllText(settingsFile, jsonText);
            }
            catch (Exception ex)
            {
                Resolver.Log.Info(ex.Message);
            }
        }
    }
}