using DwaValidatorApp.Models;
using DwaValidatorApp.Repo;
using DwaValidatorApp.Services.Interface;
using DwaValidatorApp.Viewmodel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace DwaValidatorApp
{
    /// <summary>
    /// Interaction logic for InsertDataWindow.xaml
    /// </summary>
    public partial class InsertDataWindow : Window
    {
        private readonly IValidationContextProvider _contextProvider;

        public string TargetTable { get; set; }

        public InsertDataWindow(IValidationContextProvider contextProvider)
        {
            _contextProvider = contextProvider;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DataContext = GetSuggestedData();
            }
            catch (Exception ex)
            {
                this.DataContext = new SuggestedDataVM { SuggestedDataItems = new() };
                // TODO: handle exception
                MessageBox.Show(ex.Message);
            }
        }

        private SuggestedDataVM GetSuggestedData()
        {
            var items =
                MainRepo.SuggestTestData(
                    _contextProvider.Current.ConnectionString,
                    TargetTable);

            var suggestedDataVm =
                new SuggestedDataVM
                {
                    SuggestedDataItems = new ObservableCollection<SuggestedTestData>(items)
                };

            return suggestedDataVm;
        }

        private void InsertButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var suggestedDataVm = this.DataContext as SuggestedDataVM;
                InsertSuggestedData(suggestedDataVm);

                DialogResult = true;
            }
            catch (Exception)
            {
                // TODO: Handle exception
            }
        }

        private void InsertSuggestedData(SuggestedDataVM? dataVm)
        {
            var testData = dataVm.SuggestedDataItems.OfType<TestData>();

            MainRepo.InsertTestData(
                _contextProvider.Current.ConnectionString,
                TargetTable,
                testData);
        }

        private void InsertAndNextButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var suggestedDataVm = this.DataContext as SuggestedDataVM;
                InsertSuggestedData(suggestedDataVm);

                this.DataContext = GetSuggestedData();
            }
            catch (Exception)
            {
                // TODO: Handle exception
            }

        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
