using Shiny;
using Shiny.Locations;
using System;
using System.Linq;
using TrickleToTide.Common;
using TrickleToTide.Mobile.Interfaces;
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
                StartCommand.ChangeCanExecute();
                StopCommand.ChangeCanExecute();
            });

            MessagingCenter.Subscribe<IGpsManager>(this, Constants.Message.GPS_STATE_CHANGED, (sender) => {
                if (_updates.IsRunning && !sender.IsListening)
                {
                    _updates.Stop();
                }
                StartCommand.ChangeCanExecute();
            });

            MessagingCenter.Subscribe<PositionUpdate[]>(this, Constants.Message.POSITIONS_UPDATED, (positions)=> {
                Log.Event($"{positions.Count()} positions");
            });
        }



        private Command _startCommand;
        public Command StartCommand => _startCommand ?? (_startCommand = new Command(_ => _updates.Start(), _ => !_updates.IsRunning && _updates.IsGpsConnected));

        private Command _stopCommand;
        public Command StopCommand => _stopCommand ?? (_stopCommand = new Command(_ => _updates.Stop(), _ => _updates.IsRunning && _updates.IsGpsConnected));
    }
}
