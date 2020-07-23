using Shiny;
using Shiny.Locations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TrickleToTide.Common;
using TrickleToTide.Mobile.Delegates;
using TrickleToTide.Mobile.Interfaces;
using TrickleToTide.Mobile.Services;
using Xamarin.Forms;

namespace TrickleToTide.Mobile.ViewModels
{
    class SettingsViewModel : BaseViewModel
    {
        private readonly IGpsManager _gpsManager;

        public SettingsViewModel()
        {
            _gpsManager = ShinyHost.Resolve<IGpsManager>();

            MessagingCenter.Subscribe<GpsDelegate, IGpsReading>(this, "OnReading", async (sender, reading) =>
            {
                try
                {
                    await Api.UpdatePositionAsync(new PositionUpdate() { 
                        Lat = reading.Position.Latitude,
                        Lon = reading.Position.Longitude,
                        Alt = reading.Altitude,
                        Timestamp = DateTime.Now
                    });
                    LastUpdateOn = DateTime.Now;
                }
                catch
                {
                    // noop
                }
            });
        }

        public bool IsTracking => _gpsManager.IsListening;
        private Command _booshCommand;
        public Command BooshCommand => _booshCommand ?? (_booshCommand = new Command(Boosh));
        private async void Boosh()
        {
            //await _notificationManager.Send("Test", "Message");
            await ToggleTrackingAsync();
        }


        private DateTime? _lastUpdateOn;
        public DateTime? LastUpdateOn
        {
            get { return _lastUpdateOn; }
            set { SetProperty(ref _lastUpdateOn, value); }
        }


        private async Task ToggleTrackingAsync()
        {
            if (_gpsManager.IsListening)
            {
                await _gpsManager.StopListener();
            }
            else
            {
                await _gpsManager.RequestAccessAndStart(new GpsRequest()
                {
                    Interval = TimeSpan.FromSeconds(10),
                    Priority = GpsPriority.Highest,
                    UseBackground = true
                });
            }

            OnPropertyChanged("IsTracking");
        }
    }
}
