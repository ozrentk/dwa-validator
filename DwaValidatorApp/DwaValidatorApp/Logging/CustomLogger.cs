using Microsoft.Build.Framework;

namespace DwaValidatorApp.Logging
{
    public class CustomLogger : ILogger
    {
        public List<string> Errors { get; } = new List<string>();

        public void Initialize(IEventSource eventSource)
        {
            eventSource.ErrorRaised += (sender, e) =>
            {
                // Collect error messages
                Errors.Add($"{e.File}({e.LineNumber},{e.ColumnNumber}): error {e.Code}: {e.Message}");
            };
        }

        public void LogMessage(string message)
        {
            Errors.Add(message);
        }

        public void Shutdown()
        {
            // Perform any necessary cleanup here
        }

        public LoggerVerbosity Verbosity { get; set; }
        public string Parameters { get; set; }
    }

}
