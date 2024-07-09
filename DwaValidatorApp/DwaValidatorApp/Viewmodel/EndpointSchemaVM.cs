using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DwaValidatorApp.Viewmodel
{
    public enum EndpointType 
    {
        Unknown = 0,
        Primary = 1,
        PrimarySearch = 2,
        PrimaryById = 3,
        Register = 4,
        Login = 5,
        ChangePassword = 6,
        LogsGetN = 7,
        LogsCount = 8,
        User = 9,
    }

    public enum SupportedHttpMethod
    {
        GET = 0,
        POST = 1,
        PUT = 2,
        DELETE = 3
    }

    public class EndpointSchemaVM
    {
        public IEnumerable<EndpointType> AllEndpointTypes { get; set; } = 
            Enum.GetValues<EndpointType>();
        public EndpointType EndpointType { get; set; }

        public string EndpointUri { get; set; }

        public SupportedHttpMethod HttpMethod { get; set; }

        public string PayloadSample { get; set; }

        public string QueryStringSample { get; set; }

        public static EndpointType AsEndpointType(string uri, string primary)
            => uri switch
            {
                var s when s.EndsWith("login", StringComparison.OrdinalIgnoreCase) 
                    => EndpointType.Login,
                var s when s.EndsWith("register", StringComparison.OrdinalIgnoreCase) 
                    => EndpointType.Register,
                var s when s.EndsWith("changepassword", StringComparison.OrdinalIgnoreCase)
                    => EndpointType.ChangePassword,
                var s when s.Contains("logs/get", StringComparison.OrdinalIgnoreCase)
                    => EndpointType.LogsGetN,
                var s when s.Contains("logs/count", StringComparison.OrdinalIgnoreCase)
                    => EndpointType.LogsCount,
                var s when s.EndsWith("user", StringComparison.OrdinalIgnoreCase)
                    => EndpointType.User,
                var s when s.EndsWith(primary, StringComparison.OrdinalIgnoreCase)
                    => EndpointType.Primary,
                var s when s.EndsWith($"{primary}/search", StringComparison.OrdinalIgnoreCase)
                    => EndpointType.PrimarySearch,
                var s when s.EndsWith($"{primary}/{{id}}", StringComparison.OrdinalIgnoreCase)
                    => EndpointType.PrimaryById,
                _ => EndpointType.Unknown
            };

        public static EntityType MapToEntityType(EndpointType endpointType)
            => endpointType switch
            {
                EndpointType.Primary => EntityType.Primary,
                EndpointType.PrimarySearch => EntityType.Primary,
                EndpointType.PrimaryById => EntityType.Primary,
                EndpointType.Register => EntityType.User,
                EndpointType.Login => EntityType.User,
                EndpointType.ChangePassword => EntityType.User,
                EndpointType.LogsGetN => EntityType.Unknown,
                EndpointType.LogsCount => EntityType.Unknown,
                EndpointType.User => EntityType.User,
                _ => EntityType.Unknown
            };
    }
}
