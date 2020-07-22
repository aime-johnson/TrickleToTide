using Shiny.Locations;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TrickleToTide.Mobile.Delegates
{
    public class GpsDelegate : IGpsDelegate
    {
        public Task OnReading(IGpsReading reading)
        {
            MessagingCenter.Send<GpsDelegate, IGpsReading>(this, "OnReading", reading);

            return Task.CompletedTask;
        }
    }
}
