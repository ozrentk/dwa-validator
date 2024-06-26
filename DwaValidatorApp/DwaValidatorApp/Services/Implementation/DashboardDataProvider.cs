using DwaValidatorApp.Models;
using DwaValidatorApp.Repo;
using DwaValidatorApp.Services.Interface;
using DwaValidatorApp.Tools;
using DwaValidatorApp.Viewmodel;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace DwaValidatorApp.Services.Implementation
{
    public class DashboardDataProvider : IDashboardDataProvider
    {
        private readonly IValidationContextProvider _contextProvider;

        public DashboardDataProvider(IValidationContextProvider contextProvider)
        {
            _contextProvider = contextProvider;
        }

        public async Task<IEnumerable<TableSchemaVM>> GetTableSchemas()
        {
            var tables = MainRepo.GetOrderOfTablesByDependencies(_contextProvider.Current.ConnectionString);
            //var counts = MainRepo.GetTableCounts(_contextProvider.Current.ConnectionString);

            var res = new List<TableSchemaVM>();
            foreach (var table in tables)
            {
                var comment =
                    _contextProvider.Current.DatabaseTablesAndComments.ContainsKey(table) ?
                        _contextProvider.Current.DatabaseTablesAndComments[table] :
                        null;

                //var count =
                //    counts.ContainsKey(table) ?
                //        counts[table] :
                //        0;

                var entityType = TableSchemaVM.AsEntityType(comment);

                //if (comment == null)
                //{
                //    res.AddInfo($"WARNING: Table {table} is missing a required comment");
                //}
                //else
                //{
                //    res.AddInfo($"Table {table} comment {comment} interpreted as {entityType}");
                //}

                res.Add(new TableSchemaVM
                {
                    TableName = table,
                    //Count = count,
                    EntityType = entityType
                });

                //res.AddInfo($"Table {table} count: {count} rows");
            }

            return res;

            //catch (Exception ex)
            //{
            //    res.AddInfo($"Database {context.DatabaseName} data collection failed");
            //    res.AddError(ex.Message);
            //    return res;
            //    throw ex;
            //}
        }

        public async Task<Dictionary<string, int>> GetTableCounts()
            => await MainRepo.GetTableCounts(_contextProvider.Current.ConnectionString);

        public async Task<IEnumerable<EndpointSchemaVM>> GetEndpointSchemas(string url, string primary)
        {
            var res = new List<EndpointSchemaVM>();

            try
            {
                var client = new HttpClient { BaseAddress = new Uri(url) };
                var respStream = await client.GetStreamAsync("swagger/v1/swagger.json");

                var reader = new OpenApiStreamReader();
                var result = await reader.ReadAsync(respStream);

                var jsonSamples = CreateRequestSamples(result);
                var querySamples = CreateQueryStringSamples(result);

                foreach (var path in result.OpenApiDocument.Paths.Keys)
                {
                    var pathItem = result.OpenApiDocument.Paths[path];
                    foreach (OperationType method in pathItem.Operations.Keys)
                    {
                        var opType = method.ToString();

                        var jsonSample =
                            jsonSamples.FirstOrDefault(x => x.Uri == path && x.HttpMethod == method.ToString())?.Sample;

                        var querySample =
                            querySamples.FirstOrDefault(x => x.Uri == path && x.HttpMethod == method.ToString())?.Sample;

                        var epSchema = new EndpointSchemaVM
                        {
                            HttpMethod = Enum.Parse<SupportedHttpMethod>(
                                method.ToString(),
                                true),
                            EndpointUri = path,
                            EndpointType =
                                primary == null ?
                                    EndpointType.Unknown :
                                    EndpointSchemaVM.AsEndpointType(path, primary),
                            PayloadSample = jsonSample,
                            QueryStringSample = querySample,
                        };

                        res.Add(epSchema);
                    }
                }
            }
            catch (Exception)
            {
                // TODO: handle exception
            }

            return res;
        }

        private static List<RequestSample> CreateRequestSamples(ReadResult result)
        {
            var schemaEntrySamples = new Dictionary<string, string>();
            foreach (var schemaEntry in result.OpenApiDocument.Components.Schemas)
            {
                var generated = RequestBodySampleGenerator.Generate(
                    schemaEntry.Key,
                    schemaEntry.Value);
                schemaEntrySamples.Add(schemaEntry.Value.Reference.ReferenceV2, generated.ToString());
            }

            var requestSamples = new List<RequestSample>();
            foreach (var path in result.OpenApiDocument.Paths.Keys)
            {
                var pathItem = result.OpenApiDocument.Paths[path];
                foreach (OperationType method in pathItem.Operations.Keys)
                {
                    var opType = method.ToString();
                    var opRequestContent = pathItem.Operations[method].RequestBody?.Content;
                    if (opRequestContent != null && opRequestContent.ContainsKey("application/json"))
                    {
                        var refV2 = opRequestContent["application/json"].Schema.Reference;
                        if (refV2 != null)
                        {
                            requestSamples.Add(new RequestSample
                            {
                                Uri = path,
                                HttpMethod = method.ToString(),
                                Sample = schemaEntrySamples[refV2.ReferenceV2]
                            });
                        }
                    }
                }
            }

            return requestSamples;
        }

        private static List<RequestSample> CreateQueryStringSamples(ReadResult result)
        {
            var requestSamples = new List<RequestSample>();
            foreach (var path in result.OpenApiDocument.Paths.Keys)
            {
                var pathItem = result.OpenApiDocument.Paths[path];
                foreach (var method in pathItem.Operations.Keys)
                {
                    var opType = method.ToString();

                    var queryParams = pathItem.Operations[method].Parameters
                        .Where(p => p.In == ParameterLocation.Query)
                        .ToList();

                    if (queryParams.Any())
                    {
                        var tokens = new List<string>();
                        foreach (var queryParam in queryParams)
                        {
                            var name = queryParam.Name;
                            var type = queryParam.Schema.Type;
                            var content = $"{{{type}}}"; // TODO: fix this
                            tokens.Add($"{queryParam.Name}={content}");
                        }

                        requestSamples.Add(new RequestSample
                        {
                            HttpMethod = method.ToString(),
                            Uri = path,
                            Sample = string.Join("&", tokens)
                        });
                    }
                }
            }
            return requestSamples;
        }
    }
}
