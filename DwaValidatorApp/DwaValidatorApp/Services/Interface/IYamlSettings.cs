using DwaValidatorApp.Config;

namespace DwaValidatorApp.Services.Interface
{
    public interface IYamlSettings
    {
        AppSettings Read();
        void Write(AppSettings appSettings);
    }
}
