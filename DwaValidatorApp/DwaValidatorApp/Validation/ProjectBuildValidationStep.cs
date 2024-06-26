using DwaValidatorApp.Logging;
using DwaValidatorApp.Services.Implementation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using System.Diagnostics;

namespace DwaValidatorApp.Validation
{
    public class ProjectBuildValidationStep : ValidationStepBase
    {
        protected static bool RestoreNuGetPackages(string projectFilePath, CustomLogger customLogger)
        {
            ProcessStartInfo processStartInfo = 
                new ProcessStartInfo("dotnet", $"restore \"{projectFilePath}\"")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(processStartInfo))
            {
                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();
                    customLogger.LogMessage(line); // Log standard output messages
                }

                while (!process.StandardError.EndOfStream)
                {
                    string line = process.StandardError.ReadLine();
                    customLogger.LogMessage(line); // Log error messages
                }

                process.WaitForExit();

                return process.ExitCode == 0;
            }
        }

        protected static string? BuildProject(string projectFilePath, CustomLogger customLogger)
        {
            // Configure the global properties for the build
            var globalProperties = new Dictionary<string, string>
            {
                { "Configuration", "Release" },
                { "Platform", "Any CPU" }
            };

            // Specify the build parameters
            var buildParameters = new BuildParameters
            {
                Loggers = new List<ILogger> { customLogger }
            };

            // Create a BuildRequestData instance
            var buildRequest = new BuildRequestData(projectFilePath, globalProperties, null, new[] { "Build" }, null);

            // Perform the build
            var buildResult = BuildManager.DefaultBuildManager.Build(buildParameters, buildRequest);
            if(buildResult.OverallResult == BuildResultCode.Failure)
                return null;

            var targetResult = buildResult.ResultsByTarget["Build"];
            var item = targetResult.Items.SingleOrDefault();

            return item?.ItemSpec;
        }
    }
}