using DwaValidatorApp.Services.Interface;
using DwaValidatorApp.Viewmodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DwaValidatorApp.Services.Implementation
{
    public class ValidationContextProvider : IValidationContextProvider
    {
        private ValidationContext _current = new ValidationContext();

        public ValidationContext Current
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

        public void ResetContext()
        {
            _current = new ValidationContext();
        }

    }
}
