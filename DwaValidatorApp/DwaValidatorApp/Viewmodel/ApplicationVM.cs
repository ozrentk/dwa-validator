using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DwaValidatorApp.Viewmodel
{
    public class ApplicationVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<SolutionDataVM> SolutionItems { get; set; } = new();
        public ObservableCollection<ValidationStepVM> ValidationSteps { get; set; } = new();
        public ObservableCollection<TableSchemaVM> TableSchemaItems { get; set; } = new();
        public ObservableCollection<EndpointSchemaVM> EndpointSchemaItems { get; set; } = new();
        public IEnumerable<SupportedHttpMethod> SupportedHttpMethods { get; set; } =
            Enum.GetValues<SupportedHttpMethod>();

        private ValidationStepVM _currentValidationStep;

        public ValidationStepVM CurrentValidationStep 
        {
            get
            {
                return _currentValidationStep;
            }
            set
            {
                _currentValidationStep = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentValidationStep)));
            }
        }

        private bool _isJwtKnown = false;

        public bool IsJwtKnown 
        {
            get 
            {
                return _isJwtKnown;
            }
            set 
            {
                _isJwtKnown = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsJwtKnown)));
            } 
        }
    }
}
