using DwaValidatorConsole.Services.Interface;
using DwaValidatorConsole.Validation;
using System.Data.Common;
using System.IO;

namespace DwaValidatorConsole
{
    public class AppMain
    {
        private readonly IValidator _validator;
        private readonly ValidationContext _context;

        public AppMain(IValidator validator, ValidationContext context)
        {
            _validator = validator;
            _context = context;
        }

        public async Task ExecuteAsync(
            string input,
            string output,
            bool force,
            bool trusted,
            string dataSource,
            string user,
            string pass)
        {
            _context.CurrentDirectory = AppContext.BaseDirectory;
            _context.Input = input;
            _context.Output = output;
            _context.Force = force;
            _context.DbTrustedConnection = trusted;
            _context.DbDataSource = dataSource;
            _context.DbUser = user;
            _context.DbPassword = pass;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"CurrentDirectory: {_context.CurrentDirectory}");
            Console.WriteLine($"Input: {_context.Input}");
            Console.WriteLine($"AbsoluteInput: {_context.AbsoluteInput}");
            Console.WriteLine($"Output: {_context.Output}");
            Console.WriteLine($"AbsoluteOutput: {_context.AbsoluteOutput}");
            Console.WriteLine($"UnpackTargetDatabaseDataFiles: {_context.UnpackTargetDatabaseDataFiles}");
            Console.WriteLine($"DbDataSource: {_context.DbDataSource}");
            Console.WriteLine($"DbUser: {_context.DbUser}");
            Console.WriteLine($"DbTrustedConnection: {_context.DbTrustedConnection}");
            Console.ResetColor();

            var currentValidationStep = ValidationStepKind.None;
            var lastStep = Enum.GetValues<ValidationStepKind>().Last();

            do
            {
                currentValidationStep++;

                var valRes = await _validator.ValidateAsync(currentValidationStep);
                if (valRes.IsError)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    foreach (var msg in valRes.Messages)
                    {
                        Console.WriteLine(msg);
                    }
                    Console.ResetColor();
                    return;
                }
                else 
                {
                    foreach (var msg in valRes.Messages)
                    {
                        Console.WriteLine(msg);
                    }
                }


            } while (currentValidationStep != lastStep);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"To open the solution in Visual Studio, use:\n\tstart \"{_context.UnpackTargetVsSolutionFile}\"");
            Console.ResetColor();
        }
    }
}
