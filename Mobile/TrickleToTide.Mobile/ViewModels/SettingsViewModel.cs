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

            MessagingCenter.Subscribe<IGpsManager>(this, Constants.Message.GPS_STATE_CHANGED, (sender) => {
                OnPropertyChanged("GpsConnected");
                OnPropertyChanged("ConnectCommandDescription");
            });

            MessagingCenter.Subscribe<App, Xamarin.Essentials.ConnectivityChangedEventArgs>(this, Constants.Message.CONNECTION_STATE_CHANGED, (sender, args) => {
                OnPropertyChanged("ConnectionStatus");
            });

            MessagingCenter.Subscribe<PositionUpdate[]>(this, Constants.Message.POSITIONS_UPDATED, (positions) =>
            {
                OnPropertyChanged("PositionsSummary");
                OnPropertyChanged("LastUpdate");
            });
        }

        public bool GpsConnected
        {
            get { return _locationUpdates.IsGpsConnected; }
            set
            {
                if(value != _locationUpdates.IsGpsConnected)
                {
                    if (value)
                    {
                        _locationUpdates.StartGps();
                    }
                    else
                    {
                        _locationUpdates.StopGps();
                    }
                }
            }
        }


        public string ConnectionStatus => Connectivity.NetworkAccess.ToString() + " / " + string.Join(", ", Connectivity.ConnectionProfiles.Select(x=>x.ToString()));

        public string Nickname
        {
            get { return Preferences.Get(Constants.Preferences.NICKNAME, ""); }
            set 
            { 
                Preferences.Set(Constants.Preferences.NICKNAME, value);
                OnPropertyChanged();
            }
        }


        public string[] Categories => Constants.Default.CATEGORIES;
        public string Category
        {
            get
            {
                return State.Category;
            }
            set
            {
                State.Category = value;
                OnPropertyChanged();
            }
        }

        public DateTime LastUpdate => State.LastUpdate;

        public string PositionsSummary
        {
            get
            {
                var s = new StringBuilder();
                if (State.Positions.Any())
                {
                    foreach (var category in State.Positions.GroupBy(x => x.Category).OrderBy(x => x.Key))
                    {
                        s.AppendLine($"<h3>{category.Key ?? "no-cat"}</h3>");
                        foreach (var pos in category.OrderBy(x => x.Nickname??""))
                        {
                            s.AppendLine("<div>");
                            s.AppendLine($"<span>{pos.Timestamp:HH:mm:ss}</span>");
                            s.AppendLine($"<strong> {(string.IsNullOrEmpty(pos.Nickname) ? "Anon" : pos.Nickname)}</strong>");
                            if(pos.Timestamp.Date != DateTime.Now.Date)
                            {
                                s.AppendLine($"<span> ({pos.Timestamp.ToShortDateString()})</span>");
                            }
                            s.AppendLine("</div>");
                        }
                    }
                }
                else
                {
                    s.AppendLine("Waiting for position updates.");
                }
                return s.ToString();
            }
        }
    }
}
