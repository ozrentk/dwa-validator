using DwaValidatorApp.Services.Implementation;

namespace DwaValidatorApp.Validation
{
    public class ValidationStepBase
    {
        protected ValidationStepKind Kind { get; set; }
        protected string Name { get; set; }
        protected string Description { get; set; }
        virtual public async Task<ValidationResult> RunAsync(ValidationContext context) =>
            throw new NotImplementedException();
    }
}
