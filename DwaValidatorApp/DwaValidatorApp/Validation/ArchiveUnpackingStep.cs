using DwaValidatorApp.Services.Implementation;
using System.IO;
using System.IO.Compression;

namespace DwaValidatorApp.Validation
{
    public class ArchiveUnpackingStep : ValidationStepBase
    {
        private const string RootFolder = "C:\\temp";
        private const string UnpackedArchivesFolderName = "DwaValidator\\UnpackedArchives";
        private const string DataFolderName = "DwaValidator\\Data";

        public override async Task<ValidationResult> RunAsync(ValidationContext context) =>
            await Task.Run(() =>
            {
                ValidationResult res = new();

                context.UnpackedArchivesPath =
                    Path.Combine(
                        RootFolder,
                        UnpackedArchivesFolderName);

                context.DataPath =
                    Path.Combine(
                        RootFolder,
                        DataFolderName);

                // Unzip the archive
                string archivePath = context.SolutionArchivePath;
                context.SolutionArchiveName = Path.GetFileNameWithoutExtension(archivePath);

                try
                {
                    context.ExtractedArchivePath = Path.Combine(context.UnpackedArchivesPath, context.SolutionArchiveName);
                    ZipFile.ExtractToDirectory(
                        archivePath,
                        context.ExtractedArchivePath,
                        overwriteFiles: true);
                    res.AddInfo($"Archive {archivePath} extracted to folder {context.ExtractedArchivePath}");
                }
                catch (Exception ex)
                {
                    res.AddError($"Could not extract archive: {ex.Message}");
                }

                return res;
            });
    }
}