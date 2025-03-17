using DwaValidatorApp.Logging;
using DwaValidatorApp.Services.Implementation;
using DwaValidatorApp.Tools;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;

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
                try
                {
                    KillIfRunning(apiPort);
                }
                catch (Exception ex)
                {
                    res.AddInfo($"Could not kill process on port {apiPort}: {ex.Message}");
                }
                context.WebApiProjectProcess = StartProject(context.VsWebApiProjectPath, context.WebApiProfileName, context.RedirectProcessOutputToLog, res);

                await Task.Delay(500);
                
                var mvcPort = int.Parse(context.MvcUrl.Split(":").Last());
                try
                {
                    KillIfRunning(mvcPort);
                }
                catch (Exception ex)
                {
                    res.AddInfo($"Could not kill process on port {apiPort}: {ex.Message}");
                }
                context.MvcProjectProcess = StartProject(context.VsMvcProjectPath, context.MvcProfileName, context.RedirectProcessOutputToLog, res);

                return res;
            });

        private Process StartProject(string artefact, string profileName, bool redirectOutToLog, ValidationResult res)
        {
            string workingDir = Path.GetDirectoryName(artefact);
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --launch-profile {profileName}",
                UseShellExecute = true,
                //RedirectStandardOutput = true,
                //RedirectStandardError = true,
                CreateNoWindow = false,
                WorkingDirectory = workingDir,
                ErrorDialog = true,                
            };

            if (redirectOutToLog)
            {
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                startInfo.ErrorDialog = false;
            }

            var process = Process.Start(startInfo);

            if (redirectOutToLog)
            {
                process.OutputDataReceived += (sender, e) => Debug.WriteLine("STDOUT: " + e.Data);
                process.ErrorDataReceived += (sender, e) => Debug.WriteLine("STDERR: " + e.Data);

                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();
                    //_logger.LogMessage(line); // Log standard output messages
                    res.AddInfo(line);
                }

                while (!process.StandardError.EndOfStream)
                {
                    string line = process.StandardError.ReadLine();
                    //_logger.LogMessage(line); // Log error messages
                    res.AddError(line);
                }

                process.WaitForExit();
            }

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
