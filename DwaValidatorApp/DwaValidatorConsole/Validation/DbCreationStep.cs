using System.Data.SqlClient;
using Utils;

namespace DwaValidatorConsole.Validation;

public class DbCreationStep : ValidationStepBase
{
    public override async Task<ValidationResult> RunAsync(ValidationContext context) =>
        await Task.Run(async () => await Handler(context));

    public async Task<ValidationResult> Handler(ValidationContext context)
    {
        ValidationResult res = new();

        //var instanceMdfFileName = $"{context.DatabaseName}.mdf";
        //var instanceMdfPath = FluentPath.From(context.UnpackTargetDatabaseDataFiles).AppendCombined(instanceMdfFileName).NormalizeToWindows().ToString();

        //var instanceLdfFileName = $"{context.DatabaseName}_log.ldf";
        //var instanceLdfPath = FluentPath.From(context.UnpackTargetDatabaseDataFiles).AppendCombined(instanceLdfFileName).NormalizeToWindows().ToString();

        res.AddInfo($"Database name: {context.DatabaseName}");
        //res.AddInfo($"Instance MDF file name: {instanceMdfFileName}");
        //res.AddInfo($"Instance LDF file name: {instanceLdfFileName}");

        //Directory.CreateDirectory(context.UnpackTargetDatabaseDataFiles);

        string masterConnectionString = BuildMasterConnectionString(context);

//        try
//        {
//            string dropDbScript = 
//$@"USE MASTER; 
//IF EXISTS (SELECT 1 FROM sys.databases WHERE [name] = N'[{context.DatabaseName}]')
//BEGIN
//    ALTER DATABASE [{context.DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
//    DROP DATABASE [{context.DatabaseName}]
//END";
//            await RunMasterDatabaseScriptAsync(masterConnectionString, dropDbScript);
//            res.AddInfo($"Database {context.DatabaseName} dropped");
//        }
//        catch (Exception ex)
//        {
//            res.AddError($"Database {context.DatabaseName} drop failed");
//            res.AddError(ex.Message);
//            return res;
//        }

        try
        {
            //string createDbScript = $@"
            //    CREATE DATABASE [{context.DatabaseName}] 
            //    ON (NAME = '{context.DatabaseName}', FILENAME = '{instanceMdfPath}')
            //    LOG ON (NAME = '{context.DatabaseName}_log', FILENAME = '{instanceLdfPath}');";

            string createDbScript = 
$@"USE MASTER; 
IF EXISTS (SELECT 1 FROM sys.databases WHERE [name] = N'{context.DatabaseName}')
BEGIN
    ALTER DATABASE [{context.DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
    DROP DATABASE [{context.DatabaseName}]
END
CREATE DATABASE [{context.DatabaseName}]";

            await RunMasterDatabaseScriptAsync(masterConnectionString, createDbScript);
            res.AddInfo($"Database {context.DatabaseName} (re)created");
        }
        catch (Exception ex)
        {
            res.AddError($"Database {context.DatabaseName} (re)creation failed");
            res.AddError(ex.Message);
            return res;
        }

        //var scriptBatches = new List<string>();
        //var scriptContent = File.ReadAllText(context.UnpackTargetDatabaseFile);
        //foreach (var batch in scriptContent.Split("GO"))
        //{
        //    scriptBatches.Add(batch.Trim());
        //}
        //context.SqlBatches = scriptBatches;

        context.ConnectionString = BuildInstanceConnectionString(context);

        try
        {
            await RunInstanceDatabaseScripts(context.SqlBatches, context.ConnectionString);
        }
        catch (Exception ex)
        {
            res.AddError($"Script execution failed on {context.DatabaseName}");
            res.AddError(ex.Message);
            return res;
        }

        return res;
    }

    private string BuildMasterConnectionString(ValidationContext context)
    {
        if (context.DbTrustedConnection)
        {
            return $"Data Source={context.DbDataSource};Initial Catalog=master;Integrated Security=True";
        }
        else
        {
            return $"Data Source={context.DbDataSource};Initial Catalog=master;User ID={context.DbUser};Password={context.DbPassword};TrustServerCertificate=True";
        }
    }

    private string BuildInstanceConnectionString(ValidationContext context)
    {
        return
            $@"Data Source={context.DbDataSource};" +
            $"Initial Catalog={context.DatabaseName};" +
            $"Integrated Security={(context.DbTrustedConnection ? "True" : "False")};" +
            $"{(context.DbTrustedConnection ? "" : $"User ID={context.DbUser};Password={context.DbPassword};")}" +
            $"TrustServerCertificate=True";
    }

    private async Task RunMasterDatabaseScriptAsync(string connectionString, string dbScript)
    {
        using var masterConnection = new SqlConnection(connectionString);
        await masterConnection.OpenAsync();
        using var command = new SqlCommand(dbScript, masterConnection);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task RunInstanceDatabaseScripts(List<string> scripts, string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        foreach (var script in scripts)
        {
            using var command = new SqlCommand(script, connection);
            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine($"Failed statement(s):\n{script}");
                throw;
            }
        }
    }
}
