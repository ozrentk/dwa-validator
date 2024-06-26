using DwaValidatorApp.Config;
using DwaValidatorApp.Extensions;
using DwaValidatorApp.Services.Implementation;
using DwaValidatorApp.Services.Interface;
using DwaValidatorApp.Validation;
using DwaValidatorApp.Viewmodel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace DwaValidatorApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ISolutionArchiveProvider _solutionArchiveProvider;
        private readonly AppSettings _appSettings;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IValidationContextProvider _contextProvider;
        private readonly IDashboardDataProvider _dashboardDataProvider;

        private ApplicationVM _applicationVm { get; set; } = new();

        private DispatcherTimer _procCheckTimer = new DispatcherTimer();

        public MainWindow(
            ISolutionArchiveProvider solutionArchiveProvider,
            IOptions<AppSettings> appSettings,
            IServiceScopeFactory scopeFactory,
            IValidationContextProvider contextProvider,
            IDashboardDataProvider dashboardDataProvider)
        {
            _solutionArchiveProvider = solutionArchiveProvider;
            _appSettings = appSettings.Value;
            _scopeFactory = scopeFactory;
            _contextProvider = contextProvider;
            _dashboardDataProvider = dashboardDataProvider;

            InitializeComponent();

            // Allow drop files into the window
            this.AllowDrop = true;

            // Handle the DragOver event to provide visual feedback
            this.DragOver += MainWindow_DragOver;

            // Handle the Drop event to accept the files
            this.Drop += MainWindow_Drop;

            _procCheckTimer.Tick += ProcCheckTimer_Tick;
            _procCheckTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ResetViewModel();
        }

        private void ResetViewModel()
        {
            _applicationVm.ValidationSteps.Clear();
            foreach (var step in Validator.AllValidationSteps)
            {
                _applicationVm.ValidationSteps.Add(
                    new ValidationStepVM
                    {
                        Kind = step,
                        Name = step.GetDisplayName()
                    });
            }

            _applicationVm.CurrentValidationStep = _applicationVm.ValidationSteps.First();

            DataContext = _applicationVm;
        }

        private void MainWindow_DragOver(object sender, DragEventArgs e)
        {
            // Only allow files to be dropped
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            // Get the files that were dropped
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            //_solutionArchiveProvider.AddArchives(files);

            var itemsToAdd = files.Select(x => new SolutionDataVM { SolutionPath = x });
            foreach (var item in itemsToAdd)
            {
                if (_applicationVm.SolutionItems.Contains(item))
                    continue;

                _applicationVm.SolutionItems.Add(item);
            }
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            var val = SolutionItemsListBox.SelectedValue as SolutionDataVM;
            if (val == null)
                return;

            await ValidateSolutionAsync(val);
        }

        private async Task ValidateSolutionAsync(SolutionDataVM solItem)
        {
            _contextProvider.ResetContext();
            ResetViewModel();

            using (var scope = _scopeFactory.CreateScope())
            {
                DisableButtons();

                var scopedValidator =
                    scope.ServiceProvider.GetRequiredService<IValidator>();

                // TODO: is there a better way to do this?
                var vmProvider =
                    scope.ServiceProvider.GetRequiredService<IAppVmProvider>();
                vmProvider.Current = _applicationVm;

                _contextProvider.Current.SolutionArchivePath = solItem.SolutionPath;

                var currentValidationStep = ValidationStepKind.None;
                messagesBox.Inlines.Clear();

                var lastStep = Enum.GetValues<ValidationStepKind>().Last();

                do
                {
                    currentValidationStep++;
                    _applicationVm.CurrentValidationStep =
                        _applicationVm.ValidationSteps[(int)currentValidationStep];

                    var valRes = await scopedValidator.ValidateAsync(currentValidationStep);
                    foreach (var msg in valRes.Messages)
                    {
                        messagesBox.Inlines.Add(
                            new Run($"{msg}{Environment.NewLine}")
                            {
                                Foreground = msg.IsError ? Brushes.Red : Brushes.Black
                            });
                        messagesBoxScroller.ScrollToEnd();
                    }

                    if (valRes.IsError)
                        return;

                } while (currentValidationStep != lastStep);

                // Step 1 - get table schemas
                var tableSchemas = await _dashboardDataProvider.GetTableSchemas();
                var tableCounts = await _dashboardDataProvider.GetTableCounts();

                _applicationVm.TableSchemaItems.Clear();
                foreach (var schemaItems in tableSchemas)
                {
                    if (tableCounts.ContainsKey(schemaItems.TableName))
                        schemaItems.Count = tableCounts[schemaItems.TableName];

                    _applicationVm.TableSchemaItems.Add(schemaItems);
                }

                // Step 2 - get WebAPI schemas
                _apiHttpClient = new HttpClient { BaseAddress = new Uri(_contextProvider.Current.WebApiUrl) };
                _mvcHttpClient = new HttpClient { BaseAddress = new Uri(_contextProvider.Current.MvcUrl) };
                _procCheckTimer.Start();
            }
        }

        private HttpClient _apiHttpClient;
        private HttpClient _mvcHttpClient;

        private bool _lastApiOk = false;
        private bool _lastMvcOk = false;

        private async void ProcCheckTimer_Tick(object? sender, EventArgs e)
        {
            var apiOk = await CheckProcState(
                _contextProvider.Current.WebApiProjectProcess,
                BtnProcWebApi,
                _apiHttpClient);

            if (_lastApiOk != apiOk)
            {
                OnApiStatusChanged(apiOk);
                _lastApiOk = apiOk;
            }

            var mvcOk = await CheckProcState(
                _contextProvider.Current.MvcProjectProcess,
                BtnProcMvc,
                _mvcHttpClient);

            if (_lastMvcOk != mvcOk)
            {
                OnMvcStatusChanged(mvcOk);
                _lastMvcOk = mvcOk;
            }
        }

        private async void OnApiStatusChanged(bool apiOk)
        {
            // Step 2 - get WebAPI schemas
            if (apiOk)
            {
                var primary = _applicationVm.TableSchemaItems.FirstOrDefault(x =>
                    x.EntityType == EntityType.Primary)?.TableName;
                var epSchemas = await _dashboardDataProvider.GetEndpointSchemas(
                    _contextProvider.Current.WebApiUrl,
                    primary);

                _applicationVm.EndpointSchemaItems.Clear();
                foreach (var schema in epSchemas)
                {
                    _applicationVm.EndpointSchemaItems.Add(schema);
                }
            }
        }

        private void OnMvcStatusChanged(bool mvcOk)
        {
            //var epSchemas = _dashboardDataProvider.GetEndpointSchemas();
            //foreach (var schema in epSchemas)
            //{
            //    _applicationVm.EndpointSchemaItems.Add(schema);
            //}
        }

        private static async Task<bool> CheckProcState(Process process, Button button, HttpClient client)
        {
            if (process != null)
            {
                if (process.HasExited)
                {
                    button.IsEnabled = false;
                    button.Background = new SolidColorBrush(Colors.Gray);
                }
                else
                {
                    client.CancelPendingRequests();

                    try
                    {
                        var respMsg = await client.GetAsync("/");

                        button.IsEnabled = true;
                        button.Background = new SolidColorBrush(Colors.Green);

                        return true;
                    }
                    catch (Exception)
                    {
                        button.IsEnabled = true;
                        button.Background = new SolidColorBrush(Colors.Orange);
                    }
                }
            }

            return false;
        }

        private void DisableButtons()
        {
            BtnProcWebApi.IsEnabled = false;
            BtnProcMvc.IsEnabled = false;
        }

        private async void ListBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                var listBox = sender as ListBox;
                var selectedItems = listBox.SelectedItems.OfType<SolutionDataVM>();
                foreach (var item in selectedItems.ToList())
                {
                    _applicationVm.SolutionItems.Remove(item);
                }
            }
            else if (e.Key == Key.Enter)
            {
                var listBox = sender as ListBox;
                var val = listBox.SelectedValue as SolutionDataVM;

                await ValidateSolutionAsync(val);
            }
        }

        private async void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;
            var val = listBox.SelectedValue as SolutionDataVM;

            await ValidateSolutionAsync(val);
        }

        private async void Button_InsertTestRow(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var schema = button.DataContext as TableSchemaVM;

            var insertDataWindow = new InsertDataWindow(_contextProvider);
            insertDataWindow.TargetTable = schema.TableName;

            insertDataWindow.Owner = this;
            insertDataWindow.ShowDialog();

            //var tableSchemas = await _dashboardDataProvider.GetTableSchemas();
            var tableCounts = await _dashboardDataProvider.GetTableCounts();

            //_applicationVm.TableSchemaItems.Clear();
            for (int i = 0; i < _applicationVm.TableSchemaItems.Count; i++)
            {
                var schemaItem = _applicationVm.TableSchemaItems[i];

                if (tableCounts.ContainsKey(schemaItem.TableName))
                    schemaItem.Count = tableCounts[schemaItem.TableName];
            }
        }

        private void RequestButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var schema = button.DataContext as EndpointSchemaVM;

            httpRequestMethod.SelectedValue = schema.HttpMethod;

            var builder = new UriBuilder(_contextProvider.Current.WebApiUrl);
            builder.Path = schema.EndpointUri;
            if (schema.QueryStringSample != null)
            {
                builder.Query = schema.QueryStringSample;
            }
            httpRequestUri.Text = builder.ToString();
            httpRequestUri.Text = httpRequestUri.Text.Replace("%7B", "{").Replace("%7D", "}");

            httpRequestContent.Text = schema.PayloadSample;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                mainConfigTabs.SelectedItem = executeHttpRequestTab;
            }));
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            httpResponseContent.Text = "...request sent...";

            var selectedMethod = new HttpMethod(httpRequestMethod.Text);
            var requestUri = httpRequestUri.Text;

            using (HttpClient client = new HttpClient())
            {
                var request = new HttpRequestMessage(selectedMethod, requestUri);

                if (selectedMethod == HttpMethod.Post ||
                   selectedMethod == HttpMethod.Put)
                {
                    request.Content =
                        new StringContent(
                            httpRequestContent.Text,
                            Encoding.UTF8,
                            "application/json");
                }

                if (!string.IsNullOrEmpty(_contextProvider.Current.JwtToken))
                {
                    request.Headers.Add("Authorization", $"Bearer {_contextProvider.Current.JwtToken}");
                }

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                string content = "";
                try
                {
                    var responseJson = JObject.Parse(responseBody);
                    content = responseJson.ToString(Formatting.Indented);

                }
                catch (JsonReaderException ex)
                {
                    content = responseBody;
                }

                if (content.Contains("A network-related or instance-specific error"))
                {
                    content += "\n\n" + "Are there hardcoded connection strings?";
                }

                httpResponseContent.Text = content;
                httpResponseStatusCode.Content = $"Last status: {(int)response.StatusCode} {response.ReasonPhrase}";

            }
        }

        private void AuthorizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (httpResponseContent.SelectionLength > 0)
            {
                _contextProvider.Current.JwtToken = httpResponseContent.SelectedText;
                _applicationVm.IsJwtKnown = true;
            }
            else
            {
                _contextProvider.Current.JwtToken = null;
                _applicationVm.IsJwtKnown = false;
            }
        }

        private void BtnProcMvc_Click(object sender, RoutedEventArgs e)
        {
            string programFiles = Environment.ExpandEnvironmentVariables("%ProgramFiles%");

            Process process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = $"{programFiles}\\Google\\Chrome\\Application\\chrome.exe";
            process.StartInfo.Arguments = $"{_contextProvider.Current.WebApiUrl}/swagger";
            process.Start();
        }

        private void BtnProcWebApi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string programFiles = Environment.ExpandEnvironmentVariables("%ProgramFiles%");

                Process process = new Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = $"{programFiles}\\Google\\Chrome\\Application\\chrome.exe";
                process.StartInfo.Arguments = _contextProvider.Current.MvcUrl;
                process.Start();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}