using DwaValidatorConsole.Services.Interface;
using DwaValidatorConsole.Validation;

namespace DwaValidatorConsole.Services.Implementation
{
    public class ValidationStepFactory : IValidationStepFactory
    {
        //private readonly IAppVmProvider _vmProvider;

        public ValidationStepBase Create(ValidationStepKind kind)
            => kind switch
            {
                ValidationStepKind.ZipArchiveValidation => new ZipArchiveValidationStep(),
                ValidationStepKind.FolderStructureValidation => new FolderStructureValidationStep(),
                ValidationStepKind.ArchiveUnpacking => new ArchiveUnpackingStep(),
                ValidationStepKind.SolutionFileValidation => new SolutionFileValidationStep(),
                ValidationStepKind.SqlScriptValidation => new SqlScriptValidationStep(),
                ValidationStepKind.DbCreation => new DbCreationStep(),
                ValidationStepKind.ConfigurationUpdate => new ConfigurationUpdateStep(),
                _ => throw new ArgumentException($"Unknown validation step kind: {kind}"),
            };
    }
}
