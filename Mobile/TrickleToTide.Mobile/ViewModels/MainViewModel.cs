using Shiny;
using Shiny.Locations;
using Shiny.Notifications;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TrickleToTide.Common;
using TrickleToTide.Mobile.Delegates;
using TrickleToTide.Mobile.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TrickleToTide.Mobile.ViewModels
{
    class MainViewModel : BaseViewModel
    {
        private readonly IGpsManager _gpsManager;
        private readonly Guid _id;

        public MainViewModel()
        {
            _gpsManager = ShinyHost.Resolve<IGpsManager>();

            if (!Preferences.ContainsKey("ttt-id"))
            {
                Preferences.Set("ttt-id", Guid.NewGuid().ToString());
            }

            _id = Guid.Parse(Preferences.Get("ttt-id", Guid.Empty.ToString()));

            MessagingCenter.Subscribe<GpsDelegate, IGpsReading>(this, "OnReading", async (sender, reading) =>
            {
                if (IsTracking)
                {
                    try
                    {
                        await Api.UpdatePositionAsync(new PositionUpdate()
                        {
                            Id = _id,
                            Latitude = reading.Position.Latitude,
                            Longitude = reading.Position.Longitude,
                            Altitude = reading.Altitude,
                            Accuracy = reading.PositionAccuracy,
                            Heading = reading.Heading,
                            Speed = reading.Speed,                            
                            Timestamp = reading.Timestamp,
                            Nickname = Preferences.Get("ttt-nick", "")
                        });
                        LastUpdateOn = DateTime.Now;
                    }
                    catch(Exception ex)
                    {
                        Log.Event(ex.Message);
                    }
                }

                Preferences.Set("ttt-lat", reading.Position.Latitude);
                Preferences.Set("ttt-lon", reading.Position.Longitude);
            });

            MessagingCenter.Subscribe<IGpsManager>(this, "GpsConnectionChanged", (sender) => {
                if(IsTracking && !sender.IsListening)
                {
                    IsTracking = false;
                }
                StartCommand.ChangeCanExecute();
                Log.Event($"GPS Connection listening: {sender.IsListening}");
            });

        }

        private DateTime? _lastUpdateOn;
        public DateTime? LastUpdateOn
        {
            get { return _lastUpdateOn; }
            set { SetProperty(ref _lastUpdateOn, value); }
        }


        private Command _startCommand;
        public Command StartCommand => _startCommand ?? (_startCommand = new Command(Start, CanStart));
        private bool CanStart(object arg)
        {
            return _gpsManager.IsListening && !IsTracking;
        }
        private void Start(object obj)
        {
            IsTracking = true;
            Log.Event("Start Tracking");
        }

        private Command _stopCommand;
        public Command StopCommand => _stopCommand ?? (_stopCommand = new Command(Stop, CanStop));
        private bool CanStop(object arg)
        {
            return _gpsManager.IsListening && IsTracking;
        }
        private void Stop(object obj)
        {
            IsTracking = false;
            Log.Event("Stop Tracking");
        }


        private bool _isTracking;
        public bool IsTracking
        {
            get { return _isTracking; }
            set 
            {
                if(SetProperty(ref _isTracking, value))
                {
                    OnPropertyChanged("TrackingCommandDescription");
                    StartCommand.ChangeCanExecute();
                    StopCommand.ChangeCanExecute();
                }
            }
        }
    }
}
