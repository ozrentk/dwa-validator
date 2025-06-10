using DwaValidatorConsole.Services.Interface;
using DwaValidatorConsole.Validation;

namespace DwaValidatorConsole.Services.Implementation
{
    public class Validator : IValidator
    {
        private readonly IValidationStepFactory _validationStepFactory;
        private readonly ValidationContext _validationContext;


        public Validator(
            IValidationStepFactory validationStepFactory, ValidationContext validationContext)
        {
            _validationStepFactory = validationStepFactory;
            _validationContext = validationContext;
        }

        public static ValidationStepKind[] AllValidationSteps =>
            Enum.GetValues<ValidationStepKind>();

        public async Task<ValidationResult> ValidateAsync(ValidationStepKind kind)
        {
            if (kind == 0)
            {
                return new();
            };

            var step = _validationStepFactory.Create(kind);
            return await step.RunAsync(_validationContext);
        }
    }
}
