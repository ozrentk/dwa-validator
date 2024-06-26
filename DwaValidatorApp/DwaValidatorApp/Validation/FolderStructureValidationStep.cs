using DwaValidatorApp.Services.Implementation;
using System.IO;

namespace DwaValidatorApp.Validation
{
    public class FolderStructureValidationStep : ValidationStepBase
    {
        const string solutionSufix = ".sln";

        public override async Task<ValidationResult> RunAsync(ValidationContext context) =>
            await Task.Run(() =>
            {
                ValidationResult res = new();

                string databaseFolder = Path.Combine(context.ZipArchiveRootEntry, "Database/").Replace(@"\", "/");
                string databaseSqlFile = Path.Combine(context.ZipArchiveRootEntry, "Database", "Database.sql").Replace(@"\", "/");

                var isDatabaseFolderPresent = context.ZipArchiveEntries.Contains(databaseFolder);
                if (!isDatabaseFolderPresent)
                    res.AddError($"Could not find folder {databaseFolder} in the archive");
                res.AddInfo($"{databaseFolder} folder found");

                var isDatabaseSqlFilePresent = context.ZipArchiveEntries.Contains(databaseSqlFile);
                if (!isDatabaseSqlFilePresent)
                    res.AddError($"Could not find file {databaseSqlFile} in the archive");
                res.AddInfo($"{databaseSqlFile} file found");
                context.DatabaseSqlRelativeFilePath = databaseSqlFile;

                var multipleFilesInDatabaseFolderPresent =
                    context.ZipArchiveEntries.Count(x => x.StartsWith(databaseFolder)) > 2;
                if (multipleFilesInDatabaseFolderPresent)
                    res.AddError($"Multiple files/folders present in {databaseFolder} in the archive");
                res.AddInfo($"Single file/folder found in {databaseFolder} folder");

                var solutionFiles =
                    context.ZipArchiveEntries.Where(x => x.EndsWith(".sln")).ToList();
                if (solutionFiles.Count == 0)
                    res.AddError($"No solutions present in the archive");
                else if (solutionFiles.Count > 1)
                    res.AddError($"Multiple solutions present in the archive");

                context.VsSolutionFile = solutionFiles.Single();
                res.AddInfo($"Solution file {context.VsSolutionFile} found");
                context.VsSolutionFolder = Path.GetDirectoryName(context.VsSolutionFile).Replace(@"\", "/");

                return res;
            });
    }
}
