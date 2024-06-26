using DwaValidatorApp.Logging;
using DwaValidatorApp.Services.Implementation;

namespace DwaValidatorApp.Validation
{
    public class MvcProjectBuildValidationStep : ProjectBuildValidationStep
    {
        public override async Task<ValidationResult> RunAsync(ValidationContext context) =>
            await Task.Run(() =>
            {
                ValidationResult res = new();

                var restorePackagesLogger = new CustomLogger();
                if (!RestoreNuGetPackages(context.VsMvcProjectPath, restorePackagesLogger))
                {
                    res.AddErrors(restorePackagesLogger.Errors);
                    return res;
                }
                res.AddInfo("MVC NuGet packages restored successfully.");

                var buildLogger = new CustomLogger();
                var artefact = BuildProject(context.VsMvcProjectPath, buildLogger);
                if (artefact == null)
                {
                    res.AddErrors(buildLogger.Errors);
                    return res;
                }
                res.AddInfo("MVC project built successfully.");
                res.AddInfo($"MVC artefact: {artefact}");

                context.VsMvcProjectArtefact = artefact;

                return res;
            });
    }
}
