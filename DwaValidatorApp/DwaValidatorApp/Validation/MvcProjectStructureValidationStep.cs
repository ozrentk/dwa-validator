using DwaValidatorApp.Services.Implementation;

namespace DwaValidatorApp.Validation
{
    public class MvcProjectStructureValidationStep : ProjectStructureValidationStep
    {
        public override async Task<ValidationResult> RunAsync(ValidationContext context) =>
            await Task.Run(async () =>
            {
                ValidationResult res = new();
                if (await UpdateConnectionStringFromAppsettings(res, context, context.VsMvcProjectPath))
                {
                    res.AddInfo("Connection string updated successfully");
                }
                else
                {
                    res.AddError($"Connection string update failed");
                }

                (context.MvcProfileName, context.MvcUrl) = await GetUrlFromLaunchSettings(
                    "MVC launch settings",
                    res,
                    context.VsMvcProjectPath,
                    context);

                return res;
            });
    }
}
