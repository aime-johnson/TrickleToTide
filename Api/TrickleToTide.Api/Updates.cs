using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TrickleToTide.Common;

namespace TrickleToTide.Api
{
    public static class Updates
    {
        [FunctionName("update")]
        public static async Task<IActionResult> Update(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("update");

            
            var content = await new StreamReader(req.Body).ReadToEndAsync();
            
            log.LogInformation(content);
            
            var entry = JsonConvert.DeserializeObject<PositionUpdate>(content);

            await Task.CompletedTask;
            return new OkResult();
        }


        [FunctionName("ping")]
        public static async Task<IActionResult> Ping(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("ping");

            await Task.CompletedTask;
            return new OkObjectResult($"Pong: {DateTime.UtcNow}");
        }
    }
}
