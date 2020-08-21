using Shiny.Locations;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using TrickleToTide.Mobile.Services;
using Xamarin.Forms;

namespace TrickleToTide.Mobile.Delegates
{
    public class GpsDelegate : IGpsDelegate
    {
        public async Task OnReading(IGpsReading reading)
        {
            await State.SetPositionAsync(new Common.PositionUpdate() { 
                Id = State.Id,
                Category = State.Category,
                Latitude = reading.Position.Latitude,
                Longitude = reading.Position.Longitude,
                Altitude = reading.Altitude,
                Nickname = State.Nickname,
                Timestamp = reading.Timestamp                
            });

            MessagingCenter.Send<GpsDelegate, IGpsReading>(this, Constants.Message.LOCATION_UPDATED, reading);
        }
    }
}
