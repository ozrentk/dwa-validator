using System.IO.Compression;

namespace DwaValidatorConsole.Validation
{
    public class ArchiveUnpackingStep : ValidationStepBase
    {
        public override async Task<ValidationResult> RunAsync(ValidationContext context) =>
            await Task.Run(() => Handler(context));

        public ValidationResult Handler(ValidationContext context)
        {
            ValidationResult res = new();

            // Delete the target path and unzip the archive to the target path
            context.SolutionArchiveName = Path.GetFileNameWithoutExtension(context.AbsoluteInput);

            try
            {
                //context.ExtractedArchivePath = Path.Combine(context.UnpackedArchivesPath, context.SolutionArchiveName);

                DeleteTargetPath(context.UnpackTargetRoot, res);

                ZipFile.ExtractToDirectory(
                    context.AbsoluteInput,
                    context.AbsoluteOutput,
                    overwriteFiles: true);
                res.AddInfo($"Archive {context.AbsoluteInput} extracted to folder {context.AbsoluteOutput}");
            }
            catch (Exception ex)
            {
                res.AddError($"Could not extract archive: {ex.Message}");
            }

            return res;
        }

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