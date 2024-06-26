using DwaValidatorApp.Validation;
using System.Windows.Controls;

namespace DwaValidatorApp.Viewmodel
{
    public class ValidationStepVM
    {
        public string Name { get; set; }
        public ValidationStepKind Kind { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
}
