using DwaValidatorApp.Services.Interface;
using DwaValidatorApp.Viewmodel;

namespace DwaValidatorApp.Services.Implementation
{
    public class SolutionArchiveProvider : ISolutionArchiveProvider
    {
        private List<SolutionDataVM> _archives { get; set; } = new();

        public void AddArchives(IEnumerable<string> archivePaths)
        {
            var newArchives = archivePaths.Select(x =>
                new SolutionDataVM { SolutionPath = x, StudentName = x });
            _archives.AddRange(newArchives);
        }

        public IEnumerable<SolutionDataVM> GetData()
        {
            return _archives;
        }
    }
}
