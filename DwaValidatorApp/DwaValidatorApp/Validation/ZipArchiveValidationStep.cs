using DwaValidatorApp.Services.Implementation;
using System.IO.Compression;
using System.IO;
using System.Text.RegularExpressions;

namespace DwaValidatorApp.Validation
{
    public partial class ZipArchiveValidationStep : ValidationStepBase
    {
        public override async Task<ValidationResult> RunAsync(ValidationContext context) =>
            await Task.Run(() =>
            {
                ValidationResult res = new();

                string solutionArchivePath = context.SolutionArchivePath;

                bool isZipFile = Path.GetExtension(solutionArchivePath).Equals(".zip", StringComparison.OrdinalIgnoreCase);
                if (!isZipFile)
                {
                    res.AddError($"File is not a zip archive");
                    return res;
                }
                res.AddInfo($"File {solutionArchivePath} looks like a zip archive");

                try
                {
                    using (var zipFile = ZipFile.OpenRead(solutionArchivePath))
                    {
                        context.ZipArchiveEntries = GetZipArchiveEntryNames(zipFile.Entries);
                        res.AddInfo($"File {solutionArchivePath} looks like a valid unencrypted zip archive");

                        var roots = GetZipArchiveRoots(zipFile.Entries);

                        if(roots.Count() == 0)
                        {
                            res.AddError($"No root folder found in zip archive");
                            return res;
                        }
                        else if(roots.Count() > 1)
                        {
                            res.AddError($"Multiple root folders found in zip archive");
                            return res;
                        }

                        context.ZipArchiveRootEntry = roots.Single();
                        res.AddInfo($"Root folder {context.ZipArchiveRootEntry} found in zip archive");
                    }
                }
                catch (InvalidDataException ex)
                {
                    res.AddError($"Zip archive is invalid: {ex.Message}");
                }

                return res;
            });

        public static IEnumerable<string> GetZipArchiveEntryNames(IEnumerable<ZipArchiveEntry> zipArchiveEntries)
        {
            var isEncrypted = zipArchiveEntries.Any(x => x.IsEncrypted);
            if (isEncrypted)
            {
                throw new InvalidDataException("There are encrypted files in Zip archive");
            }

            return zipArchiveEntries.Select(x => x.FullName).ToList();
        }

        [GeneratedRegex("^[\\w,\\s\\-\\.]+\\/", RegexOptions.IgnoreCase, "hr-HR")]
        private static partial Regex ArchiveRootRegex();

        public static IEnumerable<string> GetZipArchiveRoots(IEnumerable<ZipArchiveEntry> zipArchiveEntries)
        {
            var roots = new HashSet<string>();
            foreach (var entry in zipArchiveEntries)
            {
                var match = ArchiveRootRegex().Matches(entry.FullName);
                if(match.Count == 1)
                {
                    roots.Add(match[0].Value);
                }
            }

            return roots;
        }
    }
}
