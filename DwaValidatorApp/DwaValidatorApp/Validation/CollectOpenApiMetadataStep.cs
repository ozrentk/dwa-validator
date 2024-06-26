using DwaValidatorApp.Services.Implementation;
using DwaValidatorApp.Tools;
using DwaValidatorApp.Viewmodel;
using Microsoft.OpenApi.Readers;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace DwaValidatorApp.Validation
{
    public class CollectOpenApiMetadataStep : ProjectBuildValidationStep
    {
        public CollectOpenApiMetadataStep(ApplicationVM applicationVm)
        {
            
        }

        public override async Task<ValidationResult> RunAsync(ValidationContext context) =>
            await Task.Run(async () => 
            {
                ValidationResult res = new();

                // TODO: get correct URL from context
                //var client = new HttpClient { BaseAddress = new Uri("http://localhost:8080/") };
                //var respStream = await client.GetStreamAsync("swagger/v1/swagger.json");

                //var reader = new OpenApiStreamReader();
                //var result = await reader.ReadAsync(respStream);

                //var samples = new Dictionary<string, JToken>();
                //foreach (var schemaEntry in result.OpenApiDocument.Components.Schemas)
                //{
                //    var generated = JsonSchemaSampleGenerator.Generate(
                //        schemaEntry.Key, 
                //        schemaEntry.Value);
                //    samples.Add(schemaEntry.Key, generated);
                //}

                return res;
            });
    }
}