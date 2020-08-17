using Microsoft.Extensions.DependencyInjection;
using Shiny;
using System;
using System.Collections.Generic;
using System.Text;
using TrickleToTide.Mobile.Delegates;

namespace TrickleToTide.Mobile
{
    public class ShinyBoot : ShinyStartup
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            //services.UseGeofencing<GeofenceDelegate>();
            services.UseGps<GpsDelegate>();
            //services.UseMotionActivity();
            //services.UseNotifications<NotifictionDelegate>(true);
        }
    }
}
