﻿using Shiny;
using Shiny.Locations;
using Shiny.Notifications;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TrickleToTide.Common;
using TrickleToTide.Mobile.Delegates;
using TrickleToTide.Mobile.Services;
using Xamarin.Forms;

namespace TrickleToTide.Mobile.ViewModels
{
    class MainViewModel : BaseViewModel
    {
        private readonly IGpsManager _gpsManager;
        
        public MainViewModel()
        {
            _gpsManager = ShinyHost.Resolve<IGpsManager>();

            MessagingCenter.Subscribe<GpsDelegate, IGpsReading>(this, "OnReading", async (sender, reading) =>
            {
                if (IsTracking)
                {
                    try
                    {
                        await Api.UpdatePositionAsync(new PositionUpdate()
                        {
                            Lat = reading.Position.Latitude,
                            Lon = reading.Position.Longitude,
                            Alt = reading.Altitude,
                            Accuracy = reading.PositionAccuracy,
                            Heading = reading.Heading,
                            Speed = reading.Speed,                            
                            Timestamp = reading.Timestamp
                        });
                        LastUpdateOn = DateTime.Now;
                    }
                    catch
                    {
                        // noop
                    }
                }
            });

            MessagingCenter.Subscribe<IGpsManager>(this, "GpsConnectionChanged", (sender) => {
                if(IsTracking && !sender.IsListening)
                {
                    IsTracking = false;
                }
                StartCommand.ChangeCanExecute();
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
