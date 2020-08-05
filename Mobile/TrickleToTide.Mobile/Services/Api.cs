using Newtonsoft.Json;
using Shiny;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TrickleToTide.Common;
using TrickleToTide.Mobile.Interfaces;
using Xamarin.Forms;

namespace TrickleToTide.Mobile.Services
{
    public static class Api
    {
        private static readonly HttpClient _client;
        private static readonly string _endpoint;
        private static readonly int _throttleSeconds = 60;

        private static DateTime _lastUpdate = DateTime.MinValue;

        static Api()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("x-functions-key", DependencyService.Resolve<IPlatform>().ApiKey);
            _client.Timeout = TimeSpan.FromSeconds(120);

            _endpoint = DependencyService.Resolve<IPlatform>().ApiEndpoint;
        }


        public async static Task<PositionUpdate[]> UpdatePositionAsync(PositionUpdate position)
        {
            // Throttle updates
            if(_lastUpdate.AddSeconds(_throttleSeconds) < DateTime.Now)
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
                    _lastUpdate = DateTime.Now;

                    var json = await rs.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<PositionUpdate[]>(json);
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
            _lastUpdate = DateTime.MinValue;
        }
    }
}
