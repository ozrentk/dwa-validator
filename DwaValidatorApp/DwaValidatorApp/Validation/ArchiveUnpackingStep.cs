using DwaValidatorApp.Services.Implementation;
using System.IO;
using System.IO.Compression;

namespace DwaValidatorApp.Validation
{
    public class ArchiveUnpackingStep : ValidationStepBase
    {
        //private const string RootFolder = "C:\\temp\\DwaValidator";
        private const string UnpackedArchivesFolderName = "UnpackedArchives";
        private const string DataFolderName = "Data";

        public static void RecursiveDelete(DirectoryInfo baseDir)
        {
            if (!baseDir.Exists)
                return;

            foreach (var dir in baseDir.EnumerateDirectories())
            {
                RecursiveDelete(dir);
            }
            baseDir.Delete(true);
        }

        public override async Task<ValidationResult> RunAsync(ValidationContext context) =>
            await Task.Run(() =>
            {
                ValidationResult res = new();

                context.UnpackedArchivesPath =
                    Path.Combine(
                        context.Root,
                        UnpackedArchivesFolderName);

                context.DataPath =
                    Path.Combine(
                        context.Root,
                        DataFolderName);

                // Delete the target path and unzip the archive to the target path
                string archivePath = context.SolutionArchivePath;
                context.SolutionArchiveName = Path.GetFileNameWithoutExtension(archivePath);

                try
                {
                    context.ExtractedArchivePath = Path.Combine(context.UnpackedArchivesPath, context.SolutionArchiveName);

                    DeleteTargetPath(context.ExtractedArchivePath, res);

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

        private static void DeleteTargetPath(string extractedArchivePath, ValidationResult res)
        {
            if (!Directory.Exists(extractedArchivePath))
                return;

            try
            {
                Directory.Delete(extractedArchivePath, true);

                res.AddInfo($"Folder {extractedArchivePath} deleted");
            }
            catch (Exception ex)
            {
                try
                {
                    res.AddInfo($"WARNING: Folder {extractedArchivePath} delete failed, trying recursive delete...");

                    RecursiveDelete(
                        baseDir: new DirectoryInfo(extractedArchivePath));
                }
                catch (Exception ex2)
                {
                    res.AddInfo($"WARNING: Folder {extractedArchivePath} recursive delete failed, delete manually");
                    res.AddInfo(ex2.Message);
                }

                throw;
            }
        }
    }
}