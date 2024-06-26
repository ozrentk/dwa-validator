using DwaValidatorApp.Services.Implementation;

namespace DwaValidatorApp.Validation
{
    public class ApiProjectStructureValidationStep : ProjectStructureValidationStep
    {
        public override async Task<ValidationResult> RunAsync(ValidationContext context) =>
            await Task.Run(async () =>
            {
                ValidationResult res = new();
                if (await UpdateConnectionStringFromAppsettings(
                    res, 
                    context, 
                    context.VsWebApiProjectPath))
                {
                    res.AddInfo("Connection string updated successfully");
                }
                else
                {
                    res.AddError($"Connection string update failed");
                }

                (context.WebApiProfileName, context.WebApiUrl) = await GetUrlFromLaunchSettings(
                    "Web API launch settings",
                    res, 
                    context.VsWebApiProjectPath,
                    context);

                return res;
            });
    }
}
