using Newtonsoft.Json;
using Shiny;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TrickleToTide.Common;
using TrickleToTide.Mobile.Interfaces;
using TrickleToTide.Mobile.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TrickleToTide.Mobile.Services
{
    public static class State
    {
        private static Guid _id;
        private static readonly HttpClient _client;
        private static readonly string _endpoint;
        private static readonly int _throttleSeconds = 60;

        public static ObservableCollection<PositionViewModel> Positions { get; } = new ObservableCollection<PositionViewModel>();

        public static DateTime LastUpdate { get; private set; } = DateTime.MinValue;

        static State()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("x-functions-key", DependencyService.Resolve<IPlatform>().ApiKey);
            _client.Timeout = TimeSpan.FromSeconds(120);
            _endpoint = DependencyService.Resolve<IPlatform>().ApiEndpoint;
            _id = Guid.Parse(Preferences.Get(Constants.Preferences.ID, Guid.Empty.ToString()));
        }


        public async static Task<PositionUpdate[]> UpdatePositionAsync(PositionUpdate position)
        {
            // Throttle updates
            if (LastUpdate.AddSeconds(_throttleSeconds) < DateTime.Now)
            {
                Log.Event($"Update ({position.Latitude:0.000}, {position.Longitude:0.000})");
                try
                {
                    var rs = await _client.PostAsync(
                        _endpoint + "/api/update",
                        new StringContent(
                            JsonConvert.SerializeObject(position),
                            Encoding.UTF8,
                            "application/json"));

                    rs.EnsureSuccessStatusCode();
                    LastUpdate = DateTime.Now;

                    var json = await rs.Content.ReadAsStringAsync();
                    var source = JsonConvert.DeserializeObject<PositionUpdate[]>(json);

                    // Update our internal positions list
                    foreach (var pos in source)
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
                return lat == 0.0 && lon == 0.0 ? null : new Position(lat, lon);
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
                return Preferences.Get(Constants.Preferences.CATEGORY, Constants.Default.CATEGORY);
            }
            set
            {
                Preferences.Set(Constants.Preferences.CATEGORY, value);
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

        public static bool IsDev => Category == "Dev";

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
    }
}
