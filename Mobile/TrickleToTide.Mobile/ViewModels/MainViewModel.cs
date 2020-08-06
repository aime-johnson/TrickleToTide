using Shiny;
using Shiny.Locations;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using TrickleToTide.Common;
using TrickleToTide.Mobile.Interfaces;
using TrickleToTide.Mobile.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TrickleToTide.Mobile.ViewModels
{
    class MainViewModel : BaseViewModel
    {
        private readonly ILocationUpdates _updates;

        public MainViewModel()
        {
            _updates = DependencyService.Get<ILocationUpdates>();

            MessagingCenter.Subscribe<ILocationUpdates>(this, Constants.Message.TRACKING_STATE_CHANGED, (sender) => {
                UpdateStartStopAvailablilty();
            });


            MessagingCenter.Subscribe<IGpsManager>(this, Constants.Message.GPS_STATE_CHANGED, (sender) =>
            {
                if (_updates.IsRunning && !sender.IsListening)
                {
                    _updates.Stop();
                }
                UpdateStartStopAvailablilty();
            });
        }

        public ObservableCollection<PositionViewModel> Positions => State.Positions;

        private Command _startCommand;
        public Command StartCommand => _startCommand ?? (_startCommand = new Command(_ => _updates.Start(), _ => CanStart));

        private Command _stopCommand;
        public Command StopCommand => _stopCommand ?? (_stopCommand = new Command(_ => _updates.Stop(), _ => CanStop));

        public bool CanStart => !_updates.IsRunning && _updates.IsGpsConnected;
        public bool CanStop => _updates.IsRunning && _updates.IsGpsConnected;

        private void UpdateStartStopAvailablilty()
        {
            OnPropertyChanged("CanStart");
            OnPropertyChanged("CanStop");
            StartCommand.ChangeCanExecute();
            StopCommand.ChangeCanExecute();
        }
    }


    public class PositionViewModel : BaseViewModel
    {
        public Guid Id { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
    
        private Xamarin.Forms.Maps.Position _position;
        public Xamarin.Forms.Maps.Position Position
        {
            get { return _position; }
            set { SetProperty(ref _position, value); }
        }
    }
}
