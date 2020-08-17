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


        [FunctionName("history")]
        public async Task<IActionResult> History(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var id = req.Query["id"];
            if (string.IsNullOrEmpty(id))
            {
                return new NotFoundResult();
            }


            var query = _context.History
                .Where(h => h.PositionId == Guid.Parse(id))
                .OrderByDescending(h => h.Timestamp)
                .Select(h => new Common.PositionHistory() {
                    Timestamp = h.Timestamp,
                    Latitude = h.Latitude,
                    Longitude = h.Longitude,
                    Altitude = h.Altitude
                });


            var from = req.Query["fromUtc"];
            if (!string.IsNullOrEmpty(from))
            {
                query = query.Where(h => h.Timestamp > DateTime.Parse(from));
            }

            var x = await query.ToArrayAsync();

            return new OkObjectResult(x);
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
            position.Timestamp = source.Timestamp;
            
            position.Heading = 0;
            position.Accuracy = 0;
            position.Speed = 0;

            position.History.Add(new DAL.PositionHistory()
            {
                Altitude = position.Altitude,
                Timestamp = position.Timestamp,
                Latitude = position.Latitude,
                Longitude = position.Longitude,

                Heading = 0,
                Accuracy = 0,
                Speed = 0
            });

            await _context.SaveChangesAsync();
        }


        private async Task<PositionUpdate[]> GetLatestPositionsAsync()
        {
            var positions = await _context.Positions
                .Where(p => p.Category != "Dev")
                .OrderByDescending(p => p.Timestamp)
                .Select(p => new PositionUpdate()
                {
                    Id = p.Id,
                    Timestamp = p.Timestamp,
                    Altitude = p.Altitude,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    Category = p.Category,
                    Nickname = p.Nickname
                })
                .ToArrayAsync();

            return positions;
        }
    }
}
