using Meadow;
using System;
using System.IO;
using System.Text.Json;

namespace Clima_IoTHub.Model
{
    public class AppSettings
    {
        public int TestRunId { get; set; } = 0;
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
                Resolver.Log.Info($"JsonText='{jsonText}'");
                var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
                AppSettings = JsonSerializer.Deserialize<AppSettings>(jsonText, options);
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
                string jsonText = JsonSerializer.Serialize<AppSettings>(AppSettings, new JsonSerializerOptions(JsonSerializerDefaults.Web));

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