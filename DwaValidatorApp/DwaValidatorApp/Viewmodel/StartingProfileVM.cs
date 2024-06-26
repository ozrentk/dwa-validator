using Microsoft.Build.Construction;

namespace DwaValidatorApp.Viewmodel
{
    public class StartingProfileVM
    {
        public string Title { get; set; }
        public List<string> AllProfiles { get; set; }
        public string SelectedProfile { get; set; }
    }
}
