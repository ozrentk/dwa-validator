using DwaValidatorApp.Logging;
using DwaValidatorApp.Services.Implementation;

namespace DwaValidatorApp.Validation
{
    public class ApiProjectBuildValidationStep : ProjectBuildValidationStep
    {
        public override async Task<ValidationResult> RunAsync(ValidationContext context) =>
            await Task.Run(async () => 
            {
                ValidationResult res = new();

                var restorePackagesLogger = new CustomLogger();
                if (!await RestoreNuGetPackages(context.VsWebApiProjectPath, restorePackagesLogger))
                {
                    res.AddErrors(restorePackagesLogger.Errors);
                    return res;
                }
                res.AddInfo("Web API NuGet packages restored successfully.");

                var buildLogger = new CustomLogger();
                var artefact = BuildProject(context.VsWebApiProjectPath, buildLogger);
                if (artefact == null)
                {
                    res.AddErrors(buildLogger.Errors);
                    return res;
                }
                res.AddInfo("Web API project built successfully.");
                res.AddInfo($"Web API artefact: {artefact}");

                context.VsWebApiProjectArtefact = artefact;
                return res;
            });
    }
}