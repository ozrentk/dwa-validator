using DwaValidatorApp.Logging;
using DwaValidatorApp.Services.Implementation;
using DwaValidatorApp.Tools;
using System;
using System.Diagnostics;
using System.IO;

namespace DwaValidatorApp.Validation
{
    public class ProjectsStartStep : ValidationStepBase
    {
        private CustomLogger _logger = new CustomLogger();

        public override async Task<ValidationResult> RunAsync(ValidationContext context) =>
            await Task.Run(async () =>
            {
                ValidationResult res = new();

                var apiPort = int.Parse(context.WebApiUrl.Split(":").Last());
                KillIfRunning(apiPort);
                context.WebApiProjectProcess = StartProject(context.VsWebApiProjectPath, context.WebApiProfileName);

                var mvcPort = int.Parse(context.MvcUrl.Split(":").Last());
                KillIfRunning(mvcPort);
                context.MvcProjectProcess = StartProject(context.VsMvcProjectPath, context.MvcProfileName);

                return res;
            });

        private Process StartProject(string artefact, string profileName)
        {
            string workingDir = Path.GetDirectoryName(artefact);
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --launch-profile {profileName}",
                //UseShellExecute = false,
                //CreateNoWindow = true,
                //RedirectStandardOutput = true,
                //RedirectStandardError = true,
                WorkingDirectory = workingDir
            };

            var process = Process.Start(startInfo);

            //while (!process.StandardOutput.EndOfStream)
            //{
            //    string line = process.StandardOutput.ReadLine();
            //    _logger.LogMessage(line); // Log standard output messages
            //}

            //while (!process.StandardError.EndOfStream)
            //{
            //    string line = process.StandardError.ReadLine();
            //    _logger.LogMessage(line); // Log error messages
            //}

            return process;
        }

        private void KillIfRunning(int port)
        {
            var pids = ProcessFinder.GetPids(port);
            foreach (var pid in pids.Where(x => x != 0))
            {
                Process.GetProcessById(pid).Kill();
                //_logger.LogMessage($"Killed process with PID {pid}");
            }
        }
    }
}
