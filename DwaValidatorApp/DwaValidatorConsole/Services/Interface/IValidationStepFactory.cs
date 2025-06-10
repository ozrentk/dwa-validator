using DwaValidatorConsole.Validation;

namespace DwaValidatorConsole.Services.Interface
{
    public interface IValidationStepFactory
    {
        public ValidationStepBase Create(ValidationStepKind kind);
    }
}
