using Shiny.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrickleToTide.Common;
using TrickleToTide.Mobile.Delegates;
using TrickleToTide.Mobile.Services;
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

            var lastKnownPosition = State.LastKnownPosition;
            if(lastKnownPosition != null)
            {
                map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(lastKnownPosition.Latitude, lastKnownPosition.Longitude), new Distance(100)));
            }
            else
            {
                map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(Constants.Default.LATITUDE, Constants.Default.LONGITUDE), new Distance(1000)));
            }

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