using DwaValidatorApp.Validation;

namespace DwaValidatorApp.Services.Interface
{
    public interface IValidator
    {
        static ValidationStepKind[] AllValidationSteps { get; }
        Task<ValidationResult> ValidateAsync(ValidationStepKind validationStepKind);
    }
}
