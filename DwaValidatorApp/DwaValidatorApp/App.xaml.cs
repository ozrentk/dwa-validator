using DwaValidatorApp.Config;
using DwaValidatorApp.Services.Implementation;
using DwaValidatorApp.Services.Interface;
using Microsoft.Build.Locator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;

namespace DwaValidatorApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        public IConfiguration _configuration { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            MSBuildLocator.RegisterDefaults();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            _serviceProvider = serviceCollection.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(_configuration);

            // BEGIN: service registration
            services.AddSingleton<ISolutionArchiveProvider, SolutionArchiveProvider>();
            services.AddSingleton<IValidationContextProvider, ValidationContextProvider>();
            services.AddSingleton<IDashboardDataProvider, DashboardDataProvider>();
            services.AddSingleton<MainWindow>();

            services.AddScoped<IValidationStepFactory, ValidationStepFactory>();
            services.AddScoped<IValidator, Validator>();
            services.AddScoped<IAppVmProvider, AppVmProvider>();
            // END: service registration
        }
    }

}
