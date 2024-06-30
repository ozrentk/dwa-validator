using DwaValidatorApp.Config;
using DwaValidatorApp.Services.Interface;
using System.IO;
using YamlDotNet.Serialization;

namespace DwaValidatorApp.Services.Implementation
{
    public class YamlSettings : IYamlSettings
    {
        public const string SETTINGS_PATH = ".dwavalidator";

        private readonly ISerializer _yamlSerializer;
        private readonly IDeserializer _yamlDeserializer;

        public YamlSettings(ISerializer yamlSerializer, IDeserializer yamlDeserializer)
        {
            _yamlSerializer = yamlSerializer;
            _yamlDeserializer = yamlDeserializer;
        }

        public AppSettings Read()
        {
            try
            {
                var fileContent = File.ReadAllText(SETTINGS_PATH);
                var appSettings = _yamlDeserializer.Deserialize<AppSettings>(fileContent);
                return appSettings;
            }
            catch (Exception)
            {
                return new AppSettings();
            }
        }

        public void Write(AppSettings appSettings)
        {
            try
            {
                var yaml = _yamlSerializer.Serialize(appSettings);
                File.WriteAllText(SETTINGS_PATH, yaml);
            }
            catch (Exception)
            {
            }
        }
    }
}
