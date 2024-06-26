using DwaValidatorApp.Services.Implementation;

namespace DwaValidatorApp.Services.Interface
{
    public interface IValidationContextProvider
    {
        public ValidationContext Current { get; set; }
        public void ResetContext();
    }
}