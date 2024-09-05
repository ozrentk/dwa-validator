using DwaValidatorApp.Services.Implementation;
using System.IO;

namespace DwaValidatorApp.Validation
{
    public class FolderStructureValidationStep : ValidationStepBase
    {
        public override async Task<ValidationResult> RunAsync(ValidationContext context) =>
            await Task.Run(() =>
            {
                ValidationResult res = new();

                string databaseFolder = Path.Combine(context.ZipArchiveRootEntry, "Database/").Replace(@"\", "/");
                string databaseSqlFile = Path.Combine(context.ZipArchiveRootEntry, "Database", "Database.sql").Replace(@"\", "/");

                var isDatabaseFolderPresent = context.ZipArchiveEntries.Contains(databaseFolder, StringComparer.OrdinalIgnoreCase);
                if (!isDatabaseFolderPresent)
                {
                    res.AddError($"Could not find folder {databaseFolder} in the archive");
                    return res;
                }
                res.AddInfo($"{databaseFolder} folder found");

                var isDatabaseSqlFilePresent = context.ZipArchiveEntries.Contains(databaseSqlFile, StringComparer.OrdinalIgnoreCase);
                if (!isDatabaseSqlFilePresent)
                {
                    res.AddError($"Could not find file {databaseSqlFile} in the archive");
                    return res;
                }
                res.AddInfo($"{databaseSqlFile} file found");
                context.DatabaseSqlRelativeFilePath = databaseSqlFile;

                var multipleFilesInDatabaseFolderPresent =
                    context.ZipArchiveEntries.Count(x => x.StartsWith(databaseFolder, StringComparison.OrdinalIgnoreCase)) > 2;
                if (multipleFilesInDatabaseFolderPresent)
                {
                    res.AddError($"Multiple files/folders present in {databaseFolder} in the archive");
                    return res;
                }
                res.AddInfo($"Single file/folder found in {databaseFolder} folder");

                var solutionFiles =
                    context.ZipArchiveEntries.Where(x => x.EndsWith(".sln", StringComparison.OrdinalIgnoreCase)).ToList();
                if (solutionFiles.Count == 0)
                {
                    res.AddError($"No solutions present in the archive");
                    return res;
                }
                else if (solutionFiles.Count > 1)
                {
                    res.AddError($"Multiple solutions present in the archive");
                    return res;
                }

                context.VsSolutionFile = solutionFiles.Single();
                res.AddInfo($"Solution file {context.VsSolutionFile} found");
                context.VsSolutionFolder = Path.GetDirectoryName(context.VsSolutionFile).Replace(@"\", "/");

                return res;
            });
    }
}
