using DwaValidatorConsole;
using DwaValidatorConsole.Services.Implementation;
using DwaValidatorConsole.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;

var inputOption = new Option<string>("--input", "Path to the input ZIP file") { IsRequired = true };
var outputOption = new Option<string>("--output", "Path where the ZIP file will be extracted") { IsRequired = true };
var forceOption = new Option<bool>("--force", () => false, "Overwrite output folder if it already exists");

var trustedConnOption = new Option<bool>("--db_trustedconnection", () => true, "Use trusted connection for SQL Server");
var dataSourceOption = new Option<string>("--db_datasource", () => "localhost", "SQL Server instance name");
var dbUserOption = new Option<string>("--db_user", () => string.Empty, "SQL Server username");
var dbPassOption = new Option<string>("--db_password", () => string.Empty, "SQL Server password");

var rootCommand = new RootCommand("ZIP processing application");
rootCommand.AddOption(inputOption);
rootCommand.AddOption(outputOption);
rootCommand.AddOption(forceOption);
rootCommand.AddOption(trustedConnOption);
rootCommand.AddOption(dataSourceOption);
rootCommand.AddOption(dbUserOption);
rootCommand.AddOption(dbPassOption);

// DI host
using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<ValidationContext>();

        services.AddScoped<IValidationStepFactory, ValidationStepFactory>();
        services.AddScoped<IValidator, Validator>();
        services.AddScoped<AppMain>();

    })
    .Build();

rootCommand.SetHandler(
    async (string input, string output, bool force, bool trusted, string dataSource, string user, string pass) =>
    {
        if (!trusted && (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass)))
        {
            Console.Error.WriteLine("ERROR: When --db_trustedconnection is false, both --db_user and --db_password must be provided.");
            return;
        }

        var handler = host.Services.GetRequiredService<AppMain>();
        await handler.ExecuteAsync(input, output, force, trusted, dataSource, user, pass);
    },
    inputOption, outputOption, forceOption, trustedConnOption, dataSourceOption, dbUserOption, dbPassOption
);

return await rootCommand.InvokeAsync(args);
