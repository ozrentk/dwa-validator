using DwaValidatorApp.Validation;
using System.Diagnostics;

namespace DwaValidatorApp.Services.Implementation
{
    public class ValidationContext
    {
        //public List<string> InfoMessages { get; set; } = new();
        //public List<string> ErrorMessages { get; set; } = new();
        public string UnpackedArchivesPath { get; set; }
        public string DataPath { get; set; }
        public string SolutionArchivePath { get; set; }
        public string SolutionArchiveName { get; set; }
        public string ExtractedArchivePath { get; set; }
        public ValidationStepKind CurrentValidationStepKind { get; set; }
        public string ZipArchiveRootEntry { get; set; }
        public IEnumerable<string> ZipArchiveEntries { get; set; }
        public string SolutionFolder { get; set; }
        public string VsSolutionFolder { get; set; }
        public string VsSolutionFile { get; set; }
        public string VsWebApiProject { get; set; }
        public string VsWebApiProjectPath { get; set; }
        public string VsWebApiProjectFramework { get; set; }
        public string VsMvcProject { get; set; }
        public string VsMvcProjectPath { get; set; }
        public string VsMvcProjectFramework { get; set; }
        public List<string> VsOtherProjects { get; set; } = new();
        public string DatabaseSqlRelativeFilePath { get; set; }
        public Dictionary<string, string> DatabaseTablesAndComments { get; set; } = new();
        public string VsWebApiProjectConnStrName { get; set; }
        public string VsWebApiProjectConnStrValue { get; set; }
        public string VsMvcProjectConnStrName { get; set; }
        public string VsMvcProjectConnStrValue { get; set; }
        public List<string> SqlBatches { get; set; } = new();
        public string DatabaseName { get; set; }
        public string ConnectionString { get; set; }
        public string VsWebApiProjectArtefact { get; set; }
        public string VsMvcProjectArtefact { get; set; }
        public Process WebApiProjectProcess { get; set; }
        public Process MvcProjectProcess { get; set; }
        public string WebApiUrl { get; set; }
        public string MvcUrl { get; set; }
        public string WebApiProfileName { get; set; }
        public string MvcProfileName { get; set; }
        public string JwtToken { get; internal set; }
    }
}
