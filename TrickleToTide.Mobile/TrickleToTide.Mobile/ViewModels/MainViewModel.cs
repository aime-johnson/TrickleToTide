using Shiny;
using Shiny.Locations;
using Shiny.Notifications;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TrickleToTide.Mobile.Delegates;
using Xamarin.Forms;

namespace TrickleToTide.Mobile.ViewModels
{
    class MainViewModel : BaseViewModel
    {
        private readonly IGpsManager _gpsManager;
        private readonly INotificationManager _notificationManager;

        public MainViewModel()
        {
            _gpsManager = ShinyHost.Resolve<IGpsManager>();
            _notificationManager = ShinyHost.Resolve<INotificationManager>();

            MessagingCenter.Subscribe<GpsDelegate, IGpsReading>(this, "OnReading", (sender, reading) =>
            {
                LastUpdateOn = reading.Timestamp;
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
