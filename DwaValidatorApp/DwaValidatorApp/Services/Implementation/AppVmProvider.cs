using DwaValidatorApp.Services.Interface;
using DwaValidatorApp.Viewmodel;

namespace DwaValidatorApp.Services.Implementation
{
    public class AppVmProvider : IAppVmProvider
    {
        private ApplicationVM _current;

        public ApplicationVM Current
        {
            get
            {
                return _current;
            }
            set
            {
                _current = value;
            }
        }
    }
}
