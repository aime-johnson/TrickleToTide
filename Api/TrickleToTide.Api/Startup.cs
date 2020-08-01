using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using TrickleToTide.Api.DAL;

[assembly: FunctionsStartup(typeof(TrickleToTide.Api.Startup))]
namespace TrickleToTide.Api
{
    class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("settings.json", true, true)
                .AddJsonFile("local.settings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            builder.Services.AddDbContext<PositionContext>(options =>
            {
                options.UseSqlServer(config.GetConnectionString("ttt"));
            });
        }
    }
}
