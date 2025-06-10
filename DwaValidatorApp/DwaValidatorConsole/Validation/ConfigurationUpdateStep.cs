using System.Text.Json.Nodes;
using System.Text.Json;

namespace DwaValidatorConsole.Validation;

public class ConfigurationUpdateStep : ValidationStepBase
{
    public override async Task<ValidationResult> RunAsync(ValidationContext context) =>
        await Task.Run(async () => await Handler(context));

    public async Task<ValidationResult> Handler(ValidationContext context)
    {
        ValidationResult res = new();

        var unpackTargetVsSolutionRoot = Path.GetDirectoryName(context.UnpackTargetVsSolutionFile);
        var configFiles = Directory
            .EnumerateFiles(unpackTargetVsSolutionRoot, "appsettings*.json", SearchOption.AllDirectories)
            .Where(path =>
                Path.GetFileName(path).Equals("appsettings.json", StringComparison.OrdinalIgnoreCase) ||
                Path.GetFileName(path).Equals("appsettings.Development.json", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var configFile in configFiles.Where(File.Exists))
        {
            try
            {
                var jsonText = await File.ReadAllTextAsync(configFile);
                var root = JsonNode.Parse(jsonText)?.AsObject();

                if (root == null || !root.ContainsKey("ConnectionStrings"))
                {
                    res.AddInfo($"File {Path.GetFileName(configFile)} has no ConnectionStrings section");
                    continue;
                }

                var connStrSection = root["ConnectionStrings"]?.AsObject();

                if (connStrSection == null || connStrSection.Count == 0)
                {
                    res.AddInfo($"ConnectionStrings in {Path.GetFileName(configFile)} is empty");
                    continue;
                }

                if (connStrSection.Count > 1)
                {
                    res.AddError($"File {Path.GetFileName(configFile)} contains multiple connection strings");
                    continue;
                }

                // Build new connection string
                var connStrKey = connStrSection.First().Key;
                connStrSection[connStrKey] = context.ConnectionString;

                // Save file
                var newJson = JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(configFile, newJson);

                res.AddInfo($"Updated connection string in {Path.GetFileName(configFile)} (key={connStrKey})");
            }
            catch (Exception ex)
            {
                res.AddError($"Failed to process {Path.GetFileName(configFile)}: {ex.Message}");
            }
        }

        return res;
    }
}
