using Shiny;
using Shiny.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrickleToTide.Common;
using TrickleToTide.Mobile.Delegates;
using TrickleToTide.Mobile.Interfaces;
using TrickleToTide.Mobile.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TrickleToTide.Mobile.ViewModels
{
    class SettingsViewModel : BaseViewModel
    {
        private readonly ILocationUpdates _locationUpdates;

        public SettingsViewModel()
        {
            _locationUpdates = DependencyService.Resolve<ILocationUpdates>();

            MessagingCenter.Subscribe<GpsDelegate, IGpsManager>(this, "GpsConnectionChanged", (sender, reading) =>
            {
                OnPropertyChanged("GpsConnected");
                OnPropertyChanged("ConnectCommandDescription");
            });

            MessagingCenter.Subscribe<GpsDelegate, IGpsReading>(this, "OnReading", (sender, reading) =>
            {
            });

            MessagingCenter.Subscribe<App, Xamarin.Essentials.ConnectivityChangedEventArgs>(this, "ConnectionChanged", (sender, args) => {
                try
                {
                    OnPropertyChanged("ConnectionStatus");
                }
                catch
                {
                    // noop
                }
            });
        }

        public bool GpsConnected => _locationUpdates.IsGpsConnected;
        private Command _connectCommand;
        public Command ConnectCommand => _connectCommand ?? (_connectCommand = new Command(Connect));
        private void Connect()
        {
            if (_locationUpdates.IsGpsConnected)
            {
                _locationUpdates.StopGps();
            }
            else
            {
                _locationUpdates.StartGps();
            }
        }

        public string ConnectCommandDescription => _locationUpdates.IsGpsConnected ? "Disconnect GPS" : "Connect GPS";

        public string ConnectionStatus => Xamarin.Essentials.Connectivity.NetworkAccess.ToString() + " / " + string.Join(", ",Xamarin.Essentials.Connectivity.ConnectionProfiles.Select(x=>x.ToString()));

        public string Nickname
        {
            get { return Preferences.Get("ttt-nick", ""); }
            set 
            { 
                Preferences.Set("ttt-nick", value);
                OnPropertyChanged();
            }
        }
    }
}
