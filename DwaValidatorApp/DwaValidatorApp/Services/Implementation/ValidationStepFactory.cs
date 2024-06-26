using DwaValidatorApp.Services.Interface;
using DwaValidatorApp.Validation;

namespace DwaValidatorApp.Services.Implementation
{
    public class ValidationStepFactory : IValidationStepFactory
    {
        private readonly IAppVmProvider _vmProvider;

        public ValidationStepFactory(IAppVmProvider vmProvider)
        {
            _vmProvider = vmProvider;
        }

        public ValidationStepBase Create(ValidationStepKind kind)
            => kind switch
            {
                ValidationStepKind.ZipArchiveValidation => new ZipArchiveValidationStep(),
                ValidationStepKind.FolderStructureValidation => new FolderStructureValidationStep(),
                ValidationStepKind.ArchiveUnpacking => new ArchiveUnpackingStep(),
                ValidationStepKind.SolutionFileValidation => new SolutionFileValidationStep(),
                ValidationStepKind.SqlScriptValidation => new SqlScriptValidationStep(),
                ValidationStepKind.ApiProjectStructureValidation => new ApiProjectStructureValidationStep(),
                ValidationStepKind.MvcProjectStructureValidation => new MvcProjectStructureValidationStep(),
                ValidationStepKind.DbCreation => new DbCreationStep(_vmProvider.Current),
                ValidationStepKind.ApiProjectBuildValidation => new ApiProjectBuildValidationStep(),
                ValidationStepKind.MvcProjectBuildValidation => new MvcProjectBuildValidationStep(),
                ValidationStepKind.ProjectsStart => new ProjectsStartStep(),
                //ValidationStepKind.CollectDatabaseMetadata => new CollectDatabaseMetadataStep(),
                //ValidationStepKind.CollectOpenApiMetadata => new CollectOpenApiMetadataStep(_vmProvider.Current),
                //ValidationStepKind.ProvideDashboardData => new ProvideDashboardDataStep(),
                _ => throw new ArgumentException($"Unknown validation step kind: {kind}"),
            };
    }
}
