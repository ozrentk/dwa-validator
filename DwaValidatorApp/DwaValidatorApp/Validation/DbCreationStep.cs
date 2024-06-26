using DwaValidatorApp.Services.Implementation;
using DwaValidatorApp.Viewmodel;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace DwaValidatorApp.Validation
{
    public class DbCreationStep : ValidationStepBase
    {
        private readonly ApplicationVM _applicationVm;

        private string MasterLocalDbConnectionString =
            @"Data Source=(LocalDB)\MSSQLLocalDB;Integrated Security=True";

        public DbCreationStep(ApplicationVM applicationVm)
        {
            _applicationVm = applicationVm;
        }

        public override async Task<ValidationResult> RunAsync(ValidationContext context) =>
            await Task.Run(async () =>
            {
                ValidationResult res = new();

                context.DatabaseName = new String(context.SolutionArchiveName
                    .Where(ch => Char.IsLetterOrDigit(ch))
                    .ToArray());
                var instanceMdfFileName = $"{context.DatabaseName}.mdf";
                var instanceLfdFileName = $"{context.DatabaseName}_log.ldf";
                res.AddInfo($"Database name: {context.DatabaseName}");
                res.AddInfo($"Instance MDF file name: {instanceMdfFileName}");
                res.AddInfo($"Instance LDF file name: {instanceLfdFileName}");

                var instanceMdfPath = Path.Combine(context.DataPath, instanceMdfFileName);
                var instanceLdfPath = Path.Combine(context.DataPath, instanceLfdFileName);

                if (!Directory.Exists(context.DataPath))
                {
                    Directory.CreateDirectory(context.DataPath);
                }

                if (File.Exists(instanceMdfPath))
                {
                    try
                    {
                        string dropDbScript = $@"
                            USE MASTER; 
                            ALTER DATABASE [{context.DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; 
                            DROP DATABASE [{context.DatabaseName}]";
                        await RunMasterDatabaseScriptAsync(dropDbScript);
                        res.AddInfo($"Database {context.DatabaseName} dropped");
                    }
                    catch (Exception ex)
                    {
                        res.AddError($"Database {context.DatabaseName} drop failed");
                        res.AddError(ex.Message);
                        return res;
                    }
                }

                try
                {
                    string createDbScript = $@"
                        CREATE DATABASE [{context.DatabaseName}] 
                        ON (NAME = '{context.DatabaseName}', FILENAME = '{instanceMdfPath}')
                        LOG ON (NAME = '{context.DatabaseName}_log', FILENAME = '{instanceLdfPath}');";
                    await RunMasterDatabaseScriptAsync(createDbScript);
                    res.AddInfo($"Database {context.DatabaseName} created");
                }
                catch (Exception ex)
                {
                    res.AddError($"Database {context.DatabaseName} drop failed");
                    res.AddError(ex.Message);
                    return res;
                }


                var scriptBatches = context.SqlBatches;

                var scriptFolderPath = "SqlScripts";
                var scriptFiles = 
                    Directory.GetFiles(scriptFolderPath)
                             .OrderBy(x => Path.GetFileName(x))
                             .ToList();

                foreach (var scriptFile in scriptFiles)
                {
                    var scriptContent = File.ReadAllText(scriptFile);
                    scriptBatches.Add(scriptContent);
                }

                context.ConnectionString =
                    $"Data Source=(LocalDB)\\MSSQLLocalDB;Integrated Security=True;AttachDbFilename={instanceMdfPath}";
                
                try
                {
                    await RunInstanceDatabaseScripts(
                        scriptBatches,
                        context.ConnectionString);
                }
                catch (Exception ex)
                {
                    res.AddInfo($"Database {context.DatabaseName} scripts execution failed");
                    res.AddError(ex.Message);
                    return res;
                }

                return res;
            });

        private async Task RunMasterDatabaseScriptAsync(string dbScript)
        {
            using (SqlConnection masterConnection = new SqlConnection(MasterLocalDbConnectionString))
            {
                await masterConnection.OpenAsync();

                using (SqlCommand createDbCommand = new SqlCommand(dbScript, masterConnection))
                {
                    await createDbCommand.ExecuteNonQueryAsync();
                }

                await masterConnection.CloseAsync();
            }
        }

        private static async Task RunInstanceDatabaseScripts(
            List<string> scripts,
            string instanceLocalDbConnectionString)
        {
            using (SqlConnection connection = new SqlConnection(instanceLocalDbConnectionString))
            {
                await connection.OpenAsync();

                foreach (var script in scripts)
                {
                    using (SqlCommand executeScriptCommand =
                        new SqlCommand(script, connection))
                    {
                        await executeScriptCommand.ExecuteNonQueryAsync();
                    }
                }

                await connection.CloseAsync();
            }
        }
    }
}