using Newtonsoft.Json;
using Shiny;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TrickleToTide.Common;
using TrickleToTide.Mobile.Interfaces;
using TrickleToTide.Mobile.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TrickleToTide.Mobile.Services
{
    public enum TargetOption
    {
        Self,
        Selected,
        SelfAndSelected,
        All,
        None
    }

    public static class State
    {
        private static Guid _id;
        private static readonly HttpClient _client;
        private static readonly IPlatform _platform;
        private static readonly int _throttleSeconds = 60;
        private static readonly TargetOption[] _targetOptions = new[] { 
            TargetOption.All,
            TargetOption.Selected,
            TargetOption.SelfAndSelected,
            TargetOption.Self,
            TargetOption.None
        };

        public static ObservableCollection<PositionViewModel> Positions { get; } = new ObservableCollection<PositionViewModel>();

        public static DateTime LastUpdate { get; private set; } = DateTime.MinValue;

        static State()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("x-functions-key", DependencyService.Resolve<IPlatform>().ApiKey);
            _client.Timeout = TimeSpan.FromSeconds(120);
            _platform = DependencyService.Resolve<IPlatform>();
            _id = Guid.Parse(Preferences.Get(Constants.Preferences.ID, Guid.Empty.ToString()));

            SelectedTarget = TargetOption.All;
        }

        public static Guid Id
        {
            get
            {
                if (_id == default(Guid))
                {
                    if (!Preferences.ContainsKey(Constants.Preferences.ID))
                    {
                        _id = Guid.NewGuid();
                        Preferences.Set(Constants.Preferences.ID, _id.ToString());
                    }
                    else
                    {
                        _id = Guid.Parse(Preferences.Get(Constants.Preferences.ID, Guid.Empty.ToString()));
                    }
                }
                return _id;
            }
        }


        public static Task RefreshPositions()
        {
            return Task.Run(async () => {
                try
                {
                    var rs = await _client.GetAsync(_platform.ApiEndpoint + "/api/latest");

                    rs.EnsureSuccessStatusCode();

                    var json = await rs.Content.ReadAsStringAsync();
                    var source = JsonConvert.DeserializeObject<PositionUpdate[]>(json);

                    Process(source);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            });
        }


        public async static Task<PositionUpdate[]> SetPositionAsync(PositionUpdate position)
        {
            // Throttle updates
            if (LastUpdate.AddSeconds(_throttleSeconds) < DateTime.Now)
            {
                Log.Event($"Update ({position.Latitude:0.000}, {position.Longitude:0.000})");
                try
                {
                    var rs = await _client.PostAsync(
                        _platform.ApiEndpoint + "/api/update",
                        new StringContent(
                            JsonConvert.SerializeObject(position),
                            Encoding.UTF8,
                            "application/json"));

                    rs.EnsureSuccessStatusCode();
                    LastUpdate = DateTime.Now;

                    var json = await rs.Content.ReadAsStringAsync();
                    var source = JsonConvert.DeserializeObject<PositionUpdate[]>(json);

                    Process(source);

                    return source;
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }

            return null;
        }


        public static void ResetThrottle()
        {
            LastUpdate = DateTime.MinValue;
        }

        public static Position LastKnownPosition
        {
            get
            {
                var lat = Preferences.Get(Constants.Preferences.LAST_LATITUDE, 0.0);
                var lon = Preferences.Get(Constants.Preferences.LAST_LONGITUDE, 0.0);
                return lat == 0.0 && lon == 0.0 ? new Position(Constants.Default.ROUTE_CENTREPOINT.Latitude, Constants.Default.ROUTE_CENTREPOINT.Longitude) : new Position(lat, lon);
            }
            set
            {
                Preferences.Set(Constants.Preferences.LAST_LATITUDE, value.Latitude);
                Preferences.Set(Constants.Preferences.LAST_LONGITUDE, value.Longitude);
            }
        }

        public static string Category
        {
            get
            {
                // Reset user category if not recognised.
                var x = Preferences.Get(Constants.Preferences.CATEGORY, Constants.Default.CATEGORY);
                if (!Constants.Default.CATEGORIES.Contains(x))
                {
                    x = Constants.Default.CATEGORY;
                    Category = x;
                }
                return x;
            }
            set
            {
                if(value != Category)
                {
                    Preferences.Set(Constants.Preferences.CATEGORY, value);
                    MessagingCenter.Send<string>(SelectedTarget.ToString(), Constants.Message.TARGET_UPDATED);
                    Log.Event("Category changed: " + value);
                }
            }
        }


        public static string Nickname
        {
            get
            {
                return Preferences.Get(Constants.Preferences.NICKNAME, "");
            }
            set
            {
                Preferences.Set(Constants.Preferences.NICKNAME, value);
            }
        }

        private static Guid? _selectedId;
        public static Guid? SelectedId
        {
            get => _selectedId;
            set
            {
                if (_selectedId != value)
                {
                    _selectedId = value;
                    MessagingCenter.Send<string>(SelectedTarget.ToString(), Constants.Message.TARGET_UPDATED);
                }
            }
        }

        public static TargetOption SelectedTarget
        {
            get
            {
                return (TargetOption)Enum.Parse(typeof(TargetOption), Preferences.Get(Constants.Preferences.LOCATE_OPTION, TargetOption.All.ToString()));
            }
            set
            {
                if(value != SelectedTarget)
                {
                    Preferences.Set(Constants.Preferences.LOCATE_OPTION, value.ToString());
                    MessagingCenter.Send<string>(value.ToString(), Constants.Message.TARGET_UPDATED);
                    
                    _platform.Toast("Following " + Humanize(SelectedTarget));
                    Log.Event("Following " + SelectedTarget.ToString());
                    State.ResetThrottle();
                }
            }
        }

        private static string Humanize(TargetOption selectedTarget)
        {
            string s = "";
            switch (selectedTarget)
            {
                case TargetOption.All:
                    s = "all";
                    break;

                case TargetOption.Self:
                    s = "yourself";
                    break;

                case TargetOption.Selected:
                    s = Positions.Single(p => p.Id == SelectedId.GetValueOrDefault())?.Nickname;
                    break;

                case TargetOption.SelfAndSelected:
                    s = $"{Positions.Single(p => p.Id == SelectedId.GetValueOrDefault())?.Nickname} and yourself"; 
                    break;

                case TargetOption.None:
                    s = "nothing. Pan & Zoom!";
                    break;
            }
            return s;
        }

        public static void CycleTarget()
        {
            SelectedTarget = NextTarget(SelectedTarget);
        }


        private static TargetOption NextTarget(TargetOption option)
        {
            var target = option;

            while (true)
            {
                var i = Array.IndexOf(_targetOptions, target);
                if (i == _targetOptions.Count() - 1)
                {
                    i = -1;
                }

                i++;

                target = _targetOptions[i];

                // target includes the selected pin, but we don't have a selected pin. Skip.
                if((target == TargetOption.Selected || target == TargetOption.SelfAndSelected) && !SelectedId.HasValue)
                {
                    continue;
                }

                // Skip if your a dev & target is self + selected
                if(target == TargetOption.SelfAndSelected && State.Category == "Dev")
                {
                    continue;
                }

                return target;
            }
        }


        private static void Process(PositionUpdate[] source)
        {
            MainThread.BeginInvokeOnMainThread(() => {
                // Update our internal positions list
                foreach (var pos in source.Where(x => x.Category != "Dev"))
                {
                    var p = Positions.SingleOrDefault(x => x.Id == pos.Id);
                    if (p == null)
                    {
                        p = new PositionViewModel()
                        {
                            Id = pos.Id,
                            Category = pos.Category,
                            Nickname = pos.Nickname ?? "Anon",
                            Timestamp = pos.Timestamp,
                            Position = new Xamarin.Forms.Maps.Position(pos.Latitude, pos.Longitude)
                        };

                        Positions.Add(p);
                    }

                    p.Timestamp = pos.Timestamp;
                    p.Category = pos.Category;
                    p.Nickname = pos.Nickname ?? "Anon";
                    p.Position = new Xamarin.Forms.Maps.Position(pos.Latitude, pos.Longitude);
                }

                // Remove any that no longer appear in the feed
                foreach (var pos in Positions.ToArray())
                {
                    var existing = source.SingleOrDefault(x => x.Id == pos.Id);
                    if (existing == null)
                    {
                        Positions.Remove(pos);
                    }
                }

                MessagingCenter.Send<PositionUpdate[]>(source, Constants.Message.POSITIONS_UPDATED);
            });
        }
    }
}
