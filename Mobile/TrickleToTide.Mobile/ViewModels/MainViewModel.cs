using Shiny.Locations;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using TrickleToTide.Common;
using TrickleToTide.Mobile.Interfaces;
using TrickleToTide.Mobile.Services;
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
                OnPropertyChanged("Title");
                OnPropertyChanged("ShowStartHint");
                OnPropertyChanged("WaitingForPositions");
                OnPropertyChanged("SelectedPositionSummary");
            });


            MessagingCenter.Subscribe<IGpsManager>(this, Constants.Message.GPS_STATE_CHANGED, (sender) =>
            {
                if (_updates.IsRunning && !sender.IsListening)
                {
                    _updates.Stop();
                }
                UpdateStartStopAvailablilty();
                OnPropertyChanged("Title");
                OnPropertyChanged("ShowStartHint");
                OnPropertyChanged("WaitingForPositions");
                OnPropertyChanged("SelectedPositionSummary");
            });


            MessagingCenter.Subscribe<string>(this, Constants.Message.TARGET_UPDATED, (target) => {
                OnPropertyChanged("Title");
                OnPropertyChanged("SelectedPositionSummary");
            });

            MessagingCenter.Subscribe<PositionUpdate[]>(this, Constants.Message.POSITIONS_UPDATED, (positions) =>
            {
                OnPropertyChanged("WaitingForPositions");
                OnPropertyChanged("SelectedPositionSummary");
            });

            // Force a re-build
            State.Positions.Clear();
            State.ResetThrottle();
        }


        public ObservableCollection<PositionViewModel> Positions => State.Positions;
        public string Title => "Trickle to Tide" + (_updates.IsRunning ? $" (Following {State.SelectedTarget})" :"" );
        public bool WaitingForPositions => _updates.IsRunning && !Positions.Any();
        public bool CanStart => !_updates.IsRunning && _updates.IsGpsConnected;
        public bool CanStop => _updates.IsRunning && _updates.IsGpsConnected;



        private Command _startCommand;
        public Command StartCommand => _startCommand ?? (_startCommand = new Command(_ => _updates.Start(), _ => CanStart));

        private Command _stopCommand;
        public Command StopCommand => _stopCommand ?? (_stopCommand = new Command(_ => _updates.Stop(), _ => CanStop));

        private Command _setTargetCommand;
        public Command SetTargetCommand => _setTargetCommand ?? (_setTargetCommand = new Command(_ => SetTarget(), _ => CanStop));

        private PositionViewModel _selectedPosition;
        public PositionViewModel SelectedPosition 
        { 
            get => _selectedPosition;
            set
            {
                if(SetProperty(ref _selectedPosition, value))
                {
                    OnPropertyChanged("IsPositionSelected");
                    OnPropertyChanged("SelectedPositionSummary");
                }
            }
        }

        public bool IsPositionSelected => SelectedPosition != null;
        public string SelectedPositionSummary
        {
            get
            {
                var summary = new StringBuilder();

                if(SelectedPosition != null)
                {
                    foreach(var pos in Positions.Where(p=>p.Id != SelectedPosition.Id))
                    {
                        summary.AppendLine("<div>");
                        summary.AppendLine($"<strong>{pos.Nickname}</strong>: {pos.Timestamp}");
                        summary.AppendLine("</div>");
                    }
                }

                return summary.ToString();
            }
        }


        private void SetTarget()
        {
            State.CycleTarget();
        }

        private void UpdateStartStopAvailablilty()
        {
            OnPropertyChanged("CanStart");
            OnPropertyChanged("CanStop");
            StartCommand.ChangeCanExecute();
            StopCommand.ChangeCanExecute();
            SetTargetCommand.ChangeCanExecute();
        }
    }


    public class PositionViewModel : BaseViewModel
    {
        public Guid Id { get; set; }
        public string Category { get; set; }
        public string Nickname { get; set; }
        public DateTime Timestamp { get; set; }

        private Xamarin.Forms.Maps.Position _position;
        public Xamarin.Forms.Maps.Position Position
        {
            get { return _position; }
            set { SetProperty(ref _position, value); }
        }
    }
}
