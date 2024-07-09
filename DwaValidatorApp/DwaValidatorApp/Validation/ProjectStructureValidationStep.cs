using Microsoft.Build.Evaluation;
using DwaValidatorApp.Services.Implementation;
using System.Text.Json;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Build.Construction;
using System.Windows;
using DwaValidatorApp.Viewmodel;

namespace DwaValidatorApp.Validation
{
    public class ProjectStructureValidationStep : ValidationStepBase
    {
        protected static async Task<bool> UpdateConnectionStringFromAppsettings(
            ValidationResult res,
            ValidationContext context,
            string projectPath)
        {
            string appSettingsPath = GetIncludedFilePath(
                "appsettings.json",
                projectPath);

            if (appSettingsPath == null)
            {
                return false;
            }

            await UpdateExactAppSettingsInstance(res, context.DatabaseName, appSettingsPath);

            string appSettingsDevelopmentPath = GetIncludedFilePath(
                "appsettings.Development.json",
                projectPath);

            if (appSettingsDevelopmentPath != null)
            {
                await UpdateExactAppSettingsInstance(res, context.DatabaseName, appSettingsDevelopmentPath);
            }

            return true;
        }

        private static async Task UpdateExactAppSettingsInstance(ValidationResult res, string databaseName, string appSettingsPath)
        {
            var includeText = await File.ReadAllTextAsync(appSettingsPath);

            // Use Newtonsoft.Json, other approaches are not good
            var output = UpdateConnectionString(res, includeText, databaseName);

            await File.WriteAllTextAsync(appSettingsPath, output);
        }

        protected static async Task<Tuple<string, string>> GetUrlFromLaunchSettings(
            string title,
            ValidationResult res,
            string projectPath,
            ValidationContext context)
        {
            string settingsPath = GetIncludedFilePath(
                "Properties\\launchSettings.json",
                projectPath);

            if (settingsPath == null)
            {
                return null;
            }

            var includeText = await File.ReadAllTextAsync(settingsPath);

            // Use Newtonsoft.Json, other approaches are not good
            //var output = UpdateConnectionString(res, includeText, context.DatabaseName);
            JObject jsonObj = JsonConvert.DeserializeObject<JObject>(includeText);
            JObject profiles = jsonObj["profiles"] as JObject;
            if(profiles == null)
            {
                res.AddError("No profiles found in launchSettings.json");
                return null;
            }

            string profileName = null;
            JObject profile = null;
            if (profiles.ContainsKey("http"))
            {
                profileName = "http";
            }
            else
            {
                var profileNames = profiles.Properties()
                                           .Select(x => x.Name)
                                           .ToList();
                Application.Current.Dispatcher.Invoke((Action)delegate {
                    var startingProfileVm = SpecifyStartingProfile(title, profileNames);
                    profileName = startingProfileVm.SelectedProfile;
                });

                if (string.IsNullOrEmpty(profileName))
                {
                    res.AddError($"Could not detect starting profile");
                    return null;
                }

                res.AddInfo($"Profile named {profileName} is used");
            }
            profile = profiles[profileName] as JObject;

            JValue url;
            if (!profile.ContainsKey("applicationUrl"))
            {
                res.AddError("No applicationUrl found in launchSettings.json");
                return null;
            }
            else
            {
                url = profile["applicationUrl"] as JValue;
            }

            var appUrl = url.Value<string>() as string;
            if (appUrl.Contains(";"))
            {
                var appUrls = appUrl.Split(';');
                //appUrl = appUrls.FirstOrDefault(x => x.StartsWith("https"));
                //if (appUrl == null)
                //{
                //    appUrl = appUrls.FirstOrDefault(x => x.StartsWith("http"));
                //}
                appUrl = appUrls.First();
            }

            return new Tuple<string, string>(profileName, appUrl);
        }

        protected static string GetIncludedFilePath(
            string includeName,
            string projectPath)
        {
            var projectCollection = new ProjectCollection();
            var project = projectCollection.LoadProject(projectPath);

            var includeItem =
                project.Items.FirstOrDefault(i =>
                    i.EvaluatedInclude == includeName);

            if (includeItem == null)
            {
                return null;
            }

            var projectFolder = Path.GetDirectoryName(projectPath);
            var includePath = Path.Combine(
                projectFolder,
                includeItem.EvaluatedInclude);

            projectCollection.UnloadAllProjects();
            
            return includePath;
        }

        protected static string UpdateConnectionString(ValidationResult res, string jsonText, string dbName)
        {
            dynamic jsonObj = JsonConvert.DeserializeObject(jsonText);
            if (jsonObj["ConnectionStrings"] == null)
            {
                res.AddInfo("WARNING: No ConnectionStrings found in appsettings instance");
                return jsonText;
            }
            res.AddInfo("ConnectionStrings found in appsettings instance");

            var connectionStrings = jsonObj["ConnectionStrings"] as JObject;
            if (connectionStrings.Count > 1)
            {
                res.AddError("Multiple ConnectionStrings found in appsettings instance");
                return jsonText;
            }

            var cstr = connectionStrings.Children().First() as JProperty;

            var newConnectionString = $"Server=(LocalDB)\\MSSQLLocalDB;Database={dbName};Integrated Security=True";
            cstr.Value = newConnectionString;

            return JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
        }

        private static StartingProfileVM SpecifyStartingProfile(string title, List<string> profileNames)
        {
            var startProfileWindow = new StartProfileWindow();
            var startingProfileVm = new StartingProfileVM
            {
                Title = title,
                AllProfiles = profileNames
            };
            startProfileWindow.DataContext = startingProfileVm;

            //projectInSolutionWindow.Owner = this;
            startProfileWindow.ShowDialog();
            return startingProfileVm;
        }

    }
}
