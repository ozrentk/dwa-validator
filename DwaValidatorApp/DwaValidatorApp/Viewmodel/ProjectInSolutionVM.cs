using Microsoft.Build.Construction;

namespace DwaValidatorApp.Viewmodel
{
    public class ProjectInSolutionVM
    {
        public List<ProjectInSolution> AllProjects { get; set; }
        public ProjectInSolution SelectedWebApiProject { get; set; }
        public ProjectInSolution SelectedMvcProject { get; set; }
    }
}
