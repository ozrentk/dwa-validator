using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text;

namespace DwaValidatorApp.Tools
{
    public class RequestBodySampleGenerator
    {
        public RequestBodySampleGenerator()
        {
        }

        public static JToken Generate(string key, OpenApiSchema schema, HashSet<string>? ancestry = null)
        {
            ancestry ??= new();

            if (schema.Reference != null)
            {
                if (ancestry.Contains(schema.Reference.ReferenceV2))
                    return null;
                
                ancestry.Add(schema.Reference.ReferenceV2);
            }

            JToken output;

            switch (schema.Type)
            {
                case "object":
                    var jObject = new JObject();
                    if (schema.Reference != null)
                        jObject.Add("#ref", schema.Reference.ReferenceV2);

                    if (schema.Properties != null)
                    {
                        foreach (var prop in schema.Properties)
                        {
                            var newObjItem = Generate(prop.Key, prop.Value, ancestry);
                            jObject.Add(TranslateNameToJson(prop.Key), newObjItem);
                        }
                    }
                    output = jObject;
                    break;
                case "array":
                    var jArray = new JArray();
                    var newArrItem = Generate(key, schema.Items, ancestry);
                    if(newArrItem != null)
                        jArray.Add(newArrItem);
                    output = jArray;
                    break;
                case "string":
                    switch (schema.Format)
                    {
                        case "date-time":
                            output = new JValue(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));
                            break;
                        case "date":
                            output = new JValue(DateTime.Now.ToString("yyyy-MM-dd"));
                            break;
                        case "time":
                            output = new JValue(DateTime.Now.ToString("HH:mm:ss"));
                            break;
                        case "byte":
                            output = new JValue(Convert.ToBase64String(Encoding.UTF8.GetBytes("sample")));
                            break;
                        case "email":
                            output = new JValue("sample@email.com");
                            break;
                        case "uuid":
                            output = new JValue(Guid.NewGuid());
                            break;
                        case "uri":
                            output = new JValue("http://sample.com");
                            break;
                        default:
                            output = new JValue("sample");
                            break;
                    }
                    break;
                case "float":
                    output = new JValue(0.0);
                    break;
                case "integer":
                    output = new JValue(0);
                    break;
                case "number":
                    output = new JValue(0);
                    break;
                case "boolean":
                    output = new JValue(true);
                    break;
                case "null":
                    output = JValue.CreateNull();
                    break;
                default:
                    output = null;
                    break;
            }

            return output;
        }

        public static string TranslateNameToJson(string name)
        {
            return name.Substring(0, 1).ToLower() + name.Substring(1);
        }
    }
}
