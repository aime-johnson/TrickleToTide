using Shiny.Locations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TrickleToTide.Mobile.Delegates
{
    class GeofenceDelegate : IGeofenceDelegate
    {
        public Task OnStatusChanged(GeofenceState newStatus, GeofenceRegion region)
        {
            return Task.CompletedTask;
        }
    }
}
