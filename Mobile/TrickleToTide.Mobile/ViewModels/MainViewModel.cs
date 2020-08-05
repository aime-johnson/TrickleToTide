using Shiny;
using Shiny.Locations;
using System;
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

            MessagingCenter.Subscribe<ILocationUpdates>(this, "fg", (sender) => {
                StartCommand.ChangeCanExecute();
                StopCommand.ChangeCanExecute();
            });

            MessagingCenter.Subscribe<IGpsManager>(this, "GpsConnectionChanged", (sender) => {
                if (_updates.IsRunning && !sender.IsListening)
                {
                    _updates.Stop();
                }
                StartCommand.ChangeCanExecute();
            });

        }



        private Command _startCommand;
        public Command StartCommand => _startCommand ?? (_startCommand = new Command(_ => _updates.Start(), _ => !_updates.IsRunning && _updates.IsGpsConnected));

        private Command _stopCommand;
        public Command StopCommand => _stopCommand ?? (_stopCommand = new Command(_ => _updates.Stop(), _ => _updates.IsRunning && _updates.IsGpsConnected));
    }
}
