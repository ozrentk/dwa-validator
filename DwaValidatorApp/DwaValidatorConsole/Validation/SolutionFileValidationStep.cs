using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace DwaValidatorConsole.Validation;

public class SolutionFileValidationStep : ValidationStepBase
{
    public override async Task<ValidationResult> RunAsync(ValidationContext context) =>
        await Task.Run(() => Handler(context));

    public ValidationResult Handler(ValidationContext context)
    {
        var res = new ValidationResult();

        var slnPath = context.UnpackTargetVsSolutionFile;
        var slnRoot = Path.GetDirectoryName(slnPath)!;

        var projectPaths = ParseProjectsFromSln(slnPath);
        if (!projectPaths.Any())
        {
            res.AddError("No projects found in solution.");
            return res;
        }

        string? webApiProject = null, mvcProject = null;
        string? webApiPath = null, mvcPath = null;
        var otherProjects = new List<string>();

        foreach (var relativeProjectPath in projectPaths)
        {
            var fullPath = Path.GetFullPath(Path.Combine(slnRoot, relativeProjectPath));

            if (!File.Exists(fullPath))
            {
                res.AddError($"Project file not found: {fullPath}");
                continue;
            }

            var xdoc = XDocument.Load(fullPath);
            var sdkAttr = xdoc.Root?.Attribute("Sdk")?.Value ?? "";

            var projectName = Path.GetFileNameWithoutExtension(fullPath);

            if (!sdkAttr.Equals("Microsoft.NET.Sdk.Web", StringComparison.OrdinalIgnoreCase))
            {
                otherProjects.Add(projectName);
                continue;
            }

            var targetFw = xdoc.Root?
                         .Elements("PropertyGroup")
                         .Elements("TargetFramework")
                         .FirstOrDefault()?.Value;

            if (targetFw == null)
            {
                res.AddInfo($"WARNING: TargetFramework not found in {projectName}");
            }

            if (projectName.Contains("WebApi", StringComparison.OrdinalIgnoreCase))
            {
                webApiProject = projectName;
                webApiPath = fullPath;
                res.AddInfo($"Web API project found: {projectName}");
            }
            else if (projectName.Contains("WebApp", StringComparison.OrdinalIgnoreCase))
            {
                mvcProject = projectName;
                mvcPath = fullPath;
                res.AddInfo($"MVC project found: {projectName}");
            }
            else
            {
                otherProjects.Add(projectName);
                res.AddInfo($"Other web project: {projectName}");
            }
        }

        if (webApiProject == null)
        {
            res.AddInfo("Could not detect Web API project (name should contain 'WebApi')");
        }

        if (mvcProject == null)
        {
            res.AddInfo("Could not detect MVC project (name should contain 'WebApp')");
        }

        //context.VsWebApiProject = webApiProject;
        //context.VsWebApiProjectPath = webApiPath!;
        //context.VsMvcProject = mvcProject;
        //context.VsMvcProjectPath = mvcPath!;
        //context.VsOtherProjects.AddRange(otherProjects);

        // Provjera bin/obj
        var allPaths = new[] { webApiPath, mvcPath }
            .Concat(otherProjects.Select(name => Path.Combine(slnRoot, $"{name}/{name}.csproj")))
            .OfType<string>();

        foreach (var projPath in allPaths)
        {
            if (!CheckForBinObjFolders(projPath, res))
                return res;
        }

        return res;
    }

    private List<string> ParseProjectsFromSln(string slnPath)
    {
        var projectRegex = new Regex("^Project\\(\"\\{[^}]+\\}\"\\) = \"[^\"]+\", \"([^\"]+\\.csproj)\"", RegexOptions.Compiled);
        return File.ReadLines(slnPath)
                   .Select(line => projectRegex.Match(line))
                   .Where(m => m.Success)
                   .Select(m => m.Groups[1].Value.Replace('\\', Path.DirectorySeparatorChar))
                   .ToList();
    }

    private bool CheckForBinObjFolders(string projectPath, ValidationResult res)
    {
        var projectDir = Path.GetDirectoryName(projectPath)!;

        var binFolder = Path.Combine(projectDir, "bin");
        var objFolder = Path.Combine(projectDir, "obj");

        if (Directory.Exists(binFolder) || Directory.Exists(objFolder))
        {
            res.AddError($"Bin/obj folder found in {projectDir}");
            return false;
        }

        return true;
    }
}
