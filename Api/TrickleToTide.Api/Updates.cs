using System;
using System.Linq;
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
            var source = JsonConvert.DeserializeObject<PositionUpdate>(content);

            await UpdatePositionAsync(source);

            var positions = await GetLatestPositionsAsync();
            return new OkObjectResult(positions);
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


        [FunctionName("latest")]
        public async Task<IActionResult> LatestPositions(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var positions = await GetLatestPositionsAsync();
            return new OkObjectResult(positions);
        }




        private async Task UpdatePositionAsync(PositionUpdate source)
        {
            var position = await _context.Positions.SingleOrDefaultAsync(p => p.Id == source.Id);
            if (position == null)
            {
                position = new Position()
                {
                    Id = source.Id
                };
                _context.Positions.Add(position);
            }

            position.Category = source.Category;
            position.Nickname = source.Nickname;
            position.Latitude = source.Latitude;
            position.Longitude = source.Longitude;
            position.Altitude = source.Altitude;
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
        }


        private async Task<PositionUpdate[]> GetLatestPositionsAsync()
        {
            var positions = await _context.Positions.OrderByDescending(p => p.Timestamp).ToArrayAsync();
            return positions.Select(p => new PositionUpdate()
            {
                Id = p.Id,
                Timestamp = p.Timestamp,
                Accuracy = p.Accuracy,
                Altitude = p.Altitude,
                Heading = p.Heading,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                Category = p.Category,
                Nickname = p.Nickname,
                Speed = p.Speed
            }).ToArray();
        }
    }
}
