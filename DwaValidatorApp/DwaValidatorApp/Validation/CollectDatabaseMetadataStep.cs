using DwaValidatorApp.Repo;
using DwaValidatorApp.Services.Implementation;
using DwaValidatorApp.Tools;
using DwaValidatorApp.Viewmodel;
using Microsoft.OpenApi.Readers;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;

namespace DwaValidatorApp.Validation
{
    public class ProvideDashboardDataStep : ProjectBuildValidationStep
    {
        public ProvideDashboardDataStep()
        {
            
        }

        public override async Task<ValidationResult> RunAsync(ValidationContext context) =>
            await Task.Run(async () =>
            {
                ValidationResult res = new();

                try
                {
                    var tables = MainRepo.GetOrderOfTablesByDependencies(context.ConnectionString);
                    var counts = MainRepo.GetTableCounts(context.ConnectionString);

                    foreach (var table in tables)
                    {
                        var comment =
                            context.DatabaseTablesAndComments.ContainsKey(table) ?
                                context.DatabaseTablesAndComments[table] :
                                null;

                        var count =
                            counts.ContainsKey(table) ?
                                counts[table] :
                                0;

                        var entityType = TableSchemaVM.AsEntityType(comment);

                        if (comment == null)
                        {
                            res.AddInfo($"WARNING: Table {table} is missing a required comment");
                        }
                        else
                        {
                            res.AddInfo($"Table {table} comment {comment} interpreted as {entityType}");
                        }

                        var schemaVm = new TableSchemaVM
                        {
                            TableName = table,
                            Count = count,
                            EntityType = entityType
                        };

                        App.Current.Dispatcher.Invoke((Action)delegate
                        {
                            _applicationVm.TableSchemaItems.Add(schemaVm);
                        });


                        res.AddInfo($"Table {table} count: {count} rows");
                    }
                }
                catch (Exception ex)
                {
                    res.AddInfo($"Database {context.DatabaseName} data collection failed");
                    res.AddError(ex.Message);
                    return res;
                }

                return res;
            });
    }
}