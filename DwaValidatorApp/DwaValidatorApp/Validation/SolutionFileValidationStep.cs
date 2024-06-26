using DwaValidatorApp.Services.Implementation;
using DwaValidatorApp.Viewmodel;
using Microsoft.Build.Construction;
using System.IO;
using System.Windows;
using System.Xml.Linq;

namespace DwaValidatorApp.Validation
{
    public class SolutionFileValidationStep : ValidationStepBase
    {
        public override async Task<ValidationResult> RunAsync(ValidationContext context) =>
            await Task.Run(() =>
            {
                ValidationResult res = new();

                var absoluteSolutionFilePath =
                    Path.Combine(
                        context.ExtractedArchivePath,
                        context.VsSolutionFile).Replace("/", @"\");

                // Parse the solution file
                var solutionFile = SolutionFile.Parse(absoluteSolutionFilePath);

                ProjectInSolution mvcReference = null;
                //string mvcFramework = null;
                ProjectInSolution webApiReference = null;
                //string webApiFramework = null;
                List<ProjectInSolution> otherReferences = new();
                foreach (var project in solutionFile.ProjectsInOrder.Where(x => x.ProjectType == SolutionProjectType.KnownToBeMSBuildFormat))
                {
                    // Load the project file
                    var projectFile = XDocument.Load(project.AbsolutePath);

                    var targetFramework =
                        projectFile.Root
                            ?.Element("PropertyGroup")
                            ?.Element("TargetFramework")
                            ?.Value;
                    if (targetFramework == null)
                    {
                        res.AddInfo($"WARNING: Web API project framework unknown");
                    }

                    if (projectFile.Root?.Attribute("Sdk")?.Value == "Microsoft.NET.Sdk.Web")
                    {
                        bool isApi = project.ProjectName.Equals("WebApi", StringComparison.OrdinalIgnoreCase);
                        bool isMvc = project.ProjectName.Equals("WebApp", StringComparison.OrdinalIgnoreCase);

                        if (!isApi && isMvc)
                        {
                            res.AddInfo($"MVC project found: {project.ProjectName}");
                            mvcReference = project;
                        }
                        else if (isApi && !isMvc)
                        {
                            res.AddInfo($"Web API project found: {project.ProjectName}");
                            webApiReference = project;
                        }
                        else
                        {
                            res.AddInfo($"Other web project type found: {project.ProjectName}");
                            otherReferences.Add(project);
                        }
                    }
                }

                if (mvcReference == null ||
                   webApiReference == null)
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate {
                        var projectInSolutionVm = SpecifyProjectsInSolution(solutionFile, mvcReference, webApiReference);
                        mvcReference = projectInSolutionVm.SelectedMvcProject;
                        webApiReference = projectInSolutionVm.SelectedWebApiProject;
                    });
                }

                if (webApiReference?.ProjectName == null)
                {
                    res.AddError($"Could not detect Web API project (incorrect project name?)");
                    return res;
                }

                if (mvcReference?.ProjectName == null)
                {
                    res.AddError($"Could not detect MVC project (incorrect project name?)");
                    return res;
                }


                context.VsWebApiProject = webApiReference.ProjectName;
                context.VsWebApiProjectPath = webApiReference.AbsolutePath;
                context.VsMvcProject = mvcReference.ProjectName;
                context.VsMvcProjectPath = mvcReference.AbsolutePath;

                otherReferences = otherReferences.Where(x => x != mvcReference && x != webApiReference).ToList();
                context.VsOtherProjects.AddRange(otherReferences.Select(x => x.ProjectName));

                return res;
            });

        private static ProjectInSolutionVM SpecifyProjectsInSolution(SolutionFile solutionFile, ProjectInSolution mvcReference, ProjectInSolution webApiReference)
        {
            var projectInSolutionWindow = new ProjectInSolutionWindow();
            var projectInSolutionVM = new ProjectInSolutionVM
            {
                AllProjects = solutionFile.ProjectsInOrder.ToList(),
                SelectedMvcProject = mvcReference,
                SelectedWebApiProject = webApiReference
            };
            projectInSolutionWindow.DataContext = projectInSolutionVM;

            //projectInSolutionWindow.Owner = this;
            projectInSolutionWindow.ShowDialog();
            return projectInSolutionVM;
        }
    }
}
