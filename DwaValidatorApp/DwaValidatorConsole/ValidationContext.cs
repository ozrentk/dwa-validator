using System.IO;
using Utils;

public class ValidationContext : AppArguments
{
    public string CurrentDirectory { get; set; }

    public string AbsoluteInput => CurrentRootIfRelative(Input);
    public string AbsoluteOutput => CurrentRootIfRelative(Output);

    public IEnumerable<string> ArchiveEntries { get; set; }
    public string ArchiveRootEntry { get; set; }
    public string ArchiveDatabaseFolderEntry => 
        FluentPath.From("Database/").PrependCombined(ArchiveRootEntry).NormalizeToUnix().ToString();
    public string ArchiveDatabaseFileEntry =>
        FluentPath.From("Database/Database.sql").PrependCombined(ArchiveRootEntry).NormalizeToUnix().ToString();
    public string ArchiveVsSolutionEntry { get; set; }

    public string UnpackTargetRoot =>
        FluentPath.From(AbsoluteOutput).AppendCombined(ArchiveRootEntry).NormalizeToUnix().ToString();
    public string UnpackTargetDatabaseFile =>
        FluentPath.From(AbsoluteOutput).AppendCombined(ArchiveRootEntry, "Database", "Database.sql").NormalizeToUnix().ToString();
    public string UnpackTargetVsSolutionFile => //Path.Combine(AbsoluteOutput, ArchiveVsSolutionEntry).Replace(@"\", "/");
        FluentPath.From(AbsoluteOutput).AppendCombined(ArchiveVsSolutionEntry).NormalizeToUnix().ToString();
    public string UnpackTargetDatabaseDataFiles => //Path.Combine(AbsoluteOutput, "DbDataFiles");
        FluentPath.From(AbsoluteOutput).AppendCombined("DbDataFiles").NormalizeToUnix().ToString();

    public string SolutionArchiveName { get; set; }
    public List<string> SqlBatches { get; set; } = new();

    public string DatabaseName => new string(ArchiveRootEntry.Where(char.IsLetterOrDigit).ToArray());
    public string ConnectionString { get; set; }

    // Deprecated code
    public Dictionary<string, string> DatabaseTablesAndComments { get; set; } = new();

    private string CurrentRootIfRelative(string path)
    {
        var fPath = FluentPath.From(path);
        var absIn = fPath.IsRooted ? fPath : fPath.PrependCombined(CurrentDirectory);
        return absIn.GetFullPath().NormalizeToUnix().ToString();
    }
}
