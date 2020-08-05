using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrickleToTide.Api.DAL;

namespace TrickleToTide.Api
{
    class Maintanence
    {
        private readonly PositionContext _context;

        public Maintanence(PositionContext context)
        {
            _context = context;
        }

        [FunctionName("trunate-history")]
        public async Task Run([TimerTrigger("0 0 5 * * *")]TimerInfo time, ILogger log)
        {
            log.LogInformation($"Truncate position history executed at: {DateTime.Now}");
            await _context.Database.ExecuteSqlRawAsync("[ttt].[TrucateHistory]");
        }
    }
}
