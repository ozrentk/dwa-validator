using DwaValidatorApp.Validation;
using DwaValidatorApp.Viewmodel;

namespace DwaValidatorApp.Services.Interface
{
    public interface IDashboardDataProvider
    {
        public Task<IEnumerable<TableSchemaVM>> GetTableSchemas();
        public Task<Dictionary<string, int>> GetTableCounts();
        public Task<IEnumerable<EndpointSchemaVM>> GetEndpointSchemas(string url, string primary);
    }
}
