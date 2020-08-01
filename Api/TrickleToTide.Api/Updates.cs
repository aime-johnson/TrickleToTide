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
using TrickleToTide.Api.DAL;
using Microsoft.EntityFrameworkCore;

namespace TrickleToTide.Api
{
    class Updates
    {
        private readonly PositionContext _context;

        public Updates(PositionContext context)
        {
            _context = context;
        }


        [FunctionName("update")]
        public async Task<IActionResult> Update(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var content = await new StreamReader(req.Body).ReadToEndAsync();
            
            log.LogInformation(content);
            
            var source = JsonConvert.DeserializeObject<PositionUpdate>(content);

            var position = await _context.Positions.SingleOrDefaultAsync(p => p.Id == source.Id);
            if(position == null)
            {
                position = new Position()
                {
                    Id = source.Id
                };
                _context.Positions.Add(position);
            }

            position.Nickname = source.Nick;
            position.Latitude = source.Lat;
            position.Longitude = source.Lon;
            position.Altitude = source.Alt;
            position.Heading = source.Heading;
            position.Speed = source.Speed;
            position.Timestamp = source.Timestamp;
            position.Accuracy = source.Accuracy;

            position.History.Add(new PositionHistory()
            {
                Accuracy = position.Accuracy,
                Altitude = position.Altitude,
                Timestamp = position.Timestamp,
                Speed = position.Speed,
                Heading = position.Heading,
                Latitude = position.Latitude,
                Longitude = position.Longitude
            });

            await _context.SaveChangesAsync();

            return new OkResult();
        }


        private void Update(Position position, PositionUpdate source)
        {
        }

        [FunctionName("ping")]
        public async Task<IActionResult> Ping(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("ping");

            await Task.CompletedTask;
            return new OkObjectResult($"Pong: {DateTime.UtcNow}");
        }
    }
}
