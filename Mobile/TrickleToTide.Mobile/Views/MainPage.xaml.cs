using Shiny.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrickleToTide.Common;
using TrickleToTide.Mobile.Delegates;
using TrickleToTide.Mobile.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace TrickleToTide.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            BindingContext = new MainViewModel();

            var lat = Preferences.Get(Constants.Preferences.LAST_LATITUDE, 51.489271);
            var lon = Preferences.Get(Constants.Preferences.LAST_LONGITUDE, -0.235422);
            map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(lat, lon), new Distance(100)));

            MessagingCenter.Subscribe<PositionUpdate[]>(this, Constants.Message.POSITIONS_UPDATED, (positions) =>
            {
                if (positions.Any())
                {
                    var maxLat = positions.Max(p => p.Latitude);
                    var minLat = positions.Min(p => p.Latitude);
                    var maxLon = positions.Max(p => p.Longitude);
                    var minLon = positions.Min(p => p.Longitude);

                    var centreLat = (minLat + maxLat) / 2;
                    var centreLon = (minLon + maxLon) / 2;
                    var distance = GeoCode.CalcDistance(minLat, minLon, maxLat, maxLon, GeoCodeCalcMeasurement.Kilometers);

                    map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(centreLat, centreLon), Distance.FromKilometers(distance)));
                }
            });
        }
    }
}