using System.Text.Json;
using System.Text.Json.Serialization;
using BoardingSimulationV3.Calculations;
using BoardingSimulationV3.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BoardingSimulationV3
{
    public class MainFunction
    {
        private readonly ILogger<MainFunction> _logger;

        public MainFunction(ILogger<MainFunction> logger)
        {
            _logger = logger;
        }

        [Function("MainFunction")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var config = new Config();

            var calcs = new calculations();

            // update the config based on the request

            var result = calcs.runWithConfig(config);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
            };

            var responseStr = JsonSerializer.Serialize(result, options);

            // var cleanJson = RemoveEmptyObjectsAndArrays(responseStr);

            return new OkObjectResult(responseStr);
            // return new OkObjectResult(cleanJson);
        }
        public static string RemoveEmptyObjectsAndArrays(string jsonString)
        {
            // Parse the JSON string into a JObject
            var jsonObject = JObject.Parse(jsonString);

            // Recursively clean the JObject
            CleanJson(jsonObject);

            // Serialize the cleaned JObject back to a JSON string
            return JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
        }

        private static void CleanJson(JToken token)
        {
            if (token.Type == JTokenType.Object)
            {
                var obj = (JObject)token;
                var properties = obj.Properties().ToList();
                foreach (var prop in properties)
                {
                    CleanJson(prop.Value);
                    // Remove property if it is an empty object, array, or string
                    if (prop.Value.Type == JTokenType.Object && !prop.Value.HasValues
                        || prop.Value.Type == JTokenType.Array && !prop.Value.HasValues
                        || prop.Value.Type == JTokenType.String && prop.Value.ToString() == string.Empty)
                    {
                        prop.Remove();
                    }
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                var array = (JArray)token;
                var items = array.ToList();
                foreach (var item in items)
                {
                    CleanJson(item);
                    // Remove item if it is an empty object or array
                    if (item.Type == JTokenType.Object && !item.HasValues
                        || item.Type == JTokenType.Array && !item.HasValues
                        || item.Type == JTokenType.String && item.ToString() == string.Empty)
                    {
                        item.Remove();
                    }
                }
            }
        }
    }
}
