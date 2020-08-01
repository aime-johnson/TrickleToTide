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
        private readonly IGpsManager _gpsManager;

        public SettingsViewModel()
        {
            _gpsManager = ShinyHost.Resolve<IGpsManager>();

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

        public bool GpsConnected => _gpsManager.IsListening;
        private Command _connectCommand;
        public Command ConnectCommand => _connectCommand ?? (_connectCommand = new Command(Connect));
        private async void Connect()
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

            MessagingCenter.Send<IGpsManager>(_gpsManager, "GpsConnectionChanged");
            OnPropertyChanged("GpsConnected");
        }

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
