using DwaValidatorApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DwaValidatorApp.Viewmodel
{
    public class SuggestedDataVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<SuggestedTestData> SuggestedDataItems { get; set; } = new();
    }
}
