public class ZipProcessor : IZipProcessor
{
    public Task RunAsync(AppArguments args)
    {
        Console.WriteLine($"Input: {args.Input}");
        Console.WriteLine($"Output: {args.Output}");
        Console.WriteLine($"Force: {args.Force}");
        Console.WriteLine($"TrustedConn: {args.DbTrustedConnection}");
        Console.WriteLine($"DataSource: {args.DbDataSource}");
        Console.WriteLine($"User: {args.DbUser}");
        Console.WriteLine($"Pass: {args.DbPassword}");

        // TODO: implementacija obrade ZIP-a i baze

        return Task.CompletedTask;
    }
}
