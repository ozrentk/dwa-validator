using DwaValidatorConsole.Validation;

namespace DwaValidatorConsole.Services.Interface
{
    public interface IValidator
    {
        static ValidationStepKind[] AllValidationSteps { get; }
        Task<ValidationResult> ValidateAsync(ValidationStepKind validationStepKind);
    }
}
