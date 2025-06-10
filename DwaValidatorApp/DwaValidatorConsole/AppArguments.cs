public class AppArguments
{
    public string Input { get; set; } = default!;
    public string Output { get; set; } = default!;
    public bool Force { get; set; }
    public bool DbTrustedConnection { get; set; }
    public string DbDataSource { get; set; } = default!;
    public string? DbUser { get; set; }
    public string? DbPassword { get; set; }
}
