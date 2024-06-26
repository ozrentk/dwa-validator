using DwaValidatorApp.Services.Interface;
using DwaValidatorApp.Validation;

namespace DwaValidatorApp.Services.Implementation
{
    public class Validator : IValidator
    {
        private readonly IValidationStepFactory _validationStepFactory;
        private readonly IValidationContextProvider _contextProvider;

        public Validator(
            IValidationStepFactory validationStepFactory, 
            IValidationContextProvider validationContextProvider)
        {
            _validationStepFactory = validationStepFactory;
            _contextProvider = validationContextProvider;
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
            _contextProvider.Current.CurrentValidationStepKind = kind;
            return await step.RunAsync(_contextProvider.Current);
        }
    }
}
