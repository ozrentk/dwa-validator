using DwaValidatorApp.Validation;
using DwaValidatorApp.Viewmodel;

namespace DwaValidatorApp.Services.Interface
{
    public interface IAppVmProvider
    {
        public ApplicationVM Current { get; set; }
    }
}
