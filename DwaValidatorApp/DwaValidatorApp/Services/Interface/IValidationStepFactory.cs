using DwaValidatorApp.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DwaValidatorApp.Services.Interface
{
    public interface IValidationStepFactory
    {
        public ValidationStepBase Create(ValidationStepKind kind);
    }
}
