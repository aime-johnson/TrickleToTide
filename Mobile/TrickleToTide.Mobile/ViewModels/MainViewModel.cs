using Shiny.Locations;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            MessagingCenter.Subscribe<ILocationUpdates>(this, Constants.Message.TRACKING_STATE_CHANGED, (sender) => RefreshUI());
            MessagingCenter.Subscribe<string>(this, Constants.Message.TARGET_UPDATED, (target) => RefreshUI());
            MessagingCenter.Subscribe<PositionUpdate[]>(this, Constants.Message.POSITIONS_UPDATED, (positions) => RefreshUI());

            MessagingCenter.Subscribe<IGpsManager>(this, Constants.Message.GPS_STATE_CHANGED, (sender) => 
            {
                if (_updates.IsRunning && !sender.IsListening)
                {
                    _updates.Stop();
                }
                RefreshUI();
            });

            // Force a re-build
            State.Positions.Clear();
            State.ResetThrottle();

            //Task.Run(() => State.RefreshPositions());
        }


        public ObservableCollection<PositionViewModel> Positions => State.Positions;
        public string Title => "Trickle to Tide";
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
                    State.SelectedId = value?.Id;
                    if (State.SelectedId.HasValue)
                    {
                        if(State.SelectedId.Value == State.Id)
                        {
                            State.SelectedTarget = TargetOption.Self;
                        }
                        else
                        {
                            State.SelectedTarget = TargetOption.Selected;
                        }
                    }
                    else
                    {
                        // Flip back to following all, unless we were currently in pan/zoom mode
                        if(State.SelectedTarget != TargetOption.None)
                        {
                            State.SelectedTarget = TargetOption.All;
                        }
                    }

                    OnPropertyChanged("IsPositionSelected");
                    OnPropertyChanged("SelectedPositionSummary");
                    OnPropertyChanged("SelectedPositionDetail");
                }
            }
        }

        public bool IsPositionSelected => SelectedPosition != null;
        
        
        public string SelectedPositionSummary
        {
            get
            {
                var summary = new StringBuilder();
                if(SelectedPosition!= null)
                {
                    var x = SelectedPosition.CalculateDistanceTo(
                        new Xamarin.Forms.Maps.Position(State.LastKnownPosition.Latitude, State.LastKnownPosition.Longitude),
                        GeoCodeCalcMeasurement.Kilometers);

                    summary.Append($"{SelectedPosition.Nickname}");
                    if(SelectedPosition.Id != State.Id)
                    {
                        summary.Append($" ({x:0.0}km from you)");
                    }
                }
                return summary.ToString();
            }
        }


        public string SelectedPositionDetail
        {
            get
            {
                var detail = new StringBuilder();

                // Selected pin
                if(SelectedPosition != null)
                {
                    detail.AppendLine($"<div>Distance from <strong>{SelectedPosition.Nickname}</strong> to:</div>");
                    var source = Positions.Where(p => p.Id != SelectedPosition.Id && p.Category != "Dev");

                    foreach (var pos in source)
                    {
                        var x = SelectedPosition.CalculateDistanceTo(
                            pos.Position,
                            GeoCodeCalcMeasurement.Kilometers);

                        detail.AppendLine("<div>");
                        detail.AppendLine($"<strong>{pos.Nickname}:</strong> {x:0.0}km");
                        detail.AppendLine("</div>");
                    }
                }

                return detail.ToString();
            }
        }


        private void SetTarget()
        {
            State.CycleTarget();
        }


        private void RefreshUI()
        {
            AllowMapPan = State.SelectedTarget == TargetOption.None;

            OnPropertyChanged("CanStart");
            OnPropertyChanged("CanStop");
            OnPropertyChanged("Title");
            OnPropertyChanged("WaitingForPositions");
            OnPropertyChanged("SelectedPositionSummary");
            OnPropertyChanged("SelectedPositionDetail");
            StartCommand.ChangeCanExecute();
            StopCommand.ChangeCanExecute();
            SetTargetCommand.ChangeCanExecute();
        }


        private bool _allowMapPan;
        public bool AllowMapPan
        {
            get { return _allowMapPan; }
            set { SetProperty(ref _allowMapPan, value); }
        }
    }


    public class PositionViewModel : BaseViewModel
    {
        public Guid Id { get; set; }
        public string Category { get; set; }

        private string _nickName;
        public string Nickname 
        {
            get => string.IsNullOrEmpty(_nickName) ? "Anon" : _nickName;
            set { SetProperty(ref _nickName, value); }
        }

        public DateTime Timestamp { get; set; }

        private Xamarin.Forms.Maps.Position _position;
        public Xamarin.Forms.Maps.Position Position
        {
            get { return _position; }
            set { SetProperty(ref _position, value); }
        }


        public double CalculateDistanceTo(Xamarin.Forms.Maps.Position dest, GeoCodeCalcMeasurement units)
        {
            return GeoCode.CalcDistance(this.Position.Latitude, this.Position.Longitude, dest.Latitude, dest.Longitude, units);
        }
    }
}
