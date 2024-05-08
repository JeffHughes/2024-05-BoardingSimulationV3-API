using System.Text.Json;
using System.Text.Json.Serialization;
using BoardingSimulationV3.Calculations;
using BoardingSimulationV3.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BoardingSimulationV3
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function("Function1")]
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
            return new OkObjectResult(JsonSerializer.Serialize(result, options)); 
        }
    }
}
