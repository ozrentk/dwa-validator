namespace DwaValidatorConsole.Validation
{
    public class FolderStructureValidationStep : ValidationStepBase
    {
        public override async Task<ValidationResult> RunAsync(ValidationContext context) =>
            await Task.Run(() => Handler(context));

        public ValidationResult Handler(ValidationContext context)
        {
            ValidationResult res = new();

            var isDatabaseFolderPresent = context.ArchiveEntries.Contains(context.ArchiveDatabaseFolderEntry, StringComparer.OrdinalIgnoreCase);
            if (!isDatabaseFolderPresent)
            {
                res.AddError($"Could not find folder {context.ArchiveDatabaseFolderEntry} in the archive");
                return res;
            }
            res.AddInfo($"{context.ArchiveDatabaseFolderEntry} folder found in the archive");

            var isDatabaseSqlFilePresent = context.ArchiveEntries.Contains(context.ArchiveDatabaseFileEntry, StringComparer.OrdinalIgnoreCase);
            if (!isDatabaseSqlFilePresent)
            {
                res.AddError($"Could not find file {context.ArchiveDatabaseFileEntry} in the archive");
                return res;
            }
            res.AddInfo($"{context.ArchiveDatabaseFileEntry} file found in the archive");

            var multipleFilesInDatabaseFolderPresent =
                context.ArchiveEntries.Count(x => x.StartsWith(context.ArchiveDatabaseFolderEntry, StringComparison.OrdinalIgnoreCase)) > 2;
            if (multipleFilesInDatabaseFolderPresent)
            {
                res.AddError($"Multiple files/folders present in {context.ArchiveDatabaseFolderEntry} in the archive");
                return res;
            }
            res.AddInfo($"Single file/folder found in {context.ArchiveDatabaseFolderEntry} folder in the archive");

            var solutionEntries =
                context.ArchiveEntries.Where(x => x.EndsWith(".sln", StringComparison.OrdinalIgnoreCase)).ToList();
            if (solutionEntries.Count == 0)
            {
                res.AddError($"No solutions present in the archive");
                return res;
            }
            else if (solutionEntries.Count > 1)
            {
                res.AddError($"Multiple solutions present in the archive");
                return res;
            }

            context.ArchiveVsSolutionEntry = solutionEntries.Single();
            res.AddInfo($"Solution file {context.ArchiveVsSolutionEntry} found in the archive");

            return res;
        }
    }
}
