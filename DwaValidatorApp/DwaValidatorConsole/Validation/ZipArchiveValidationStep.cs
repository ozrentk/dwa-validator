using System.IO.Compression;
using System.Text.RegularExpressions;

namespace DwaValidatorConsole.Validation
{
    public partial class ZipArchiveValidationStep : ValidationStepBase
    {
        public override async Task<ValidationResult> RunAsync(ValidationContext context) =>
            await Task.Run(() => Handler(context));

        public ValidationResult Handler(ValidationContext context)
        {
            ValidationResult res = new();

            bool isZipFile = Path.GetExtension(context.AbsoluteInput).Equals(".zip", StringComparison.OrdinalIgnoreCase);
            if (!isZipFile)
            {
                res.AddError($"File is not a zip archive");
                return res;
            }
            res.AddInfo($"File {context.AbsoluteInput} looks like a zip archive");

            if (!File.Exists(context.AbsoluteInput))
            {
                res.AddError($"File doesn't exist");
                return res;
            }

            try
            {
                using (var zipFile = ZipFile.OpenRead(context.AbsoluteInput))
                {
                    var pcZipFileEntries = zipFile.Entries.Where(x => !x.FullName.StartsWith("__MACOSX"));

                    context.ArchiveEntries = GetZipArchiveEntryNames(pcZipFileEntries);
                    res.AddInfo($"File {context.AbsoluteInput} looks like a valid unencrypted zip archive");

                    var roots = GetZipArchiveRoots(pcZipFileEntries);

                    if (roots.Count() == 0)
                    {
                        res.AddError($"No root folder found in zip archive");
                        return res;
                    }
                    else if (roots.Count() > 1)
                    {
                        res.AddError($"Multiple root folders found in zip archive");
                        return res;
                    }

                    context.ArchiveRootEntry = roots.Single();
                    res.AddInfo($"Root folder {context.ArchiveRootEntry} found in zip archive");
                }
            }
            catch (InvalidDataException ex)
            {
                res.AddError($"Zip archive is invalid: {ex.Message}");
            }

            return res;
        }

        private static IEnumerable<string> GetZipArchiveEntryNames(IEnumerable<ZipArchiveEntry> zipArchiveEntries)
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

        private static IEnumerable<string> GetZipArchiveRoots(IEnumerable<ZipArchiveEntry> zipArchiveEntries)
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
