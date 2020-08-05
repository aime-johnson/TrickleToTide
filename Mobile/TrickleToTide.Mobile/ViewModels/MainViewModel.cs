using Shiny;
using Shiny.Locations;
using System;
using System.Collections.ObjectModel;
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
        private readonly ObservableCollection<PositionViewModel> _positions = new ObservableCollection<PositionViewModel>();

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
                MainThread.BeginInvokeOnMainThread(() => {
                    foreach (var pos in positions)
                    {
                        var position = _positions.SingleOrDefault(p => p.Id == pos.Id);
                        if (position == null)
                        {
                            position = new PositionViewModel()
                            {
                                Id = pos.Id,
                                Address = "address",
                                Description = pos.Nickname ?? "no-name",
                                Position = new Xamarin.Forms.Maps.Position(pos.Latitude, pos.Longitude)
                        };

                            _positions.Add(position);
                        }

                        position.Address = "address";
                        position.Description = pos.Nickname ?? "no-name";
                        position.Position = new Xamarin.Forms.Maps.Position(pos.Latitude, pos.Longitude);
                    }
                });
            });
        }

        public ObservableCollection<PositionViewModel> Positions => _positions;

        private Command _startCommand;
        public Command StartCommand => _startCommand ?? (_startCommand = new Command(_ => _updates.Start(), _ => !_updates.IsRunning && _updates.IsGpsConnected));

        private Command _stopCommand;
        public Command StopCommand => _stopCommand ?? (_stopCommand = new Command(_ => _updates.Stop(), _ => _updates.IsRunning && _updates.IsGpsConnected));
    }


    class PositionViewModel : BaseViewModel
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
