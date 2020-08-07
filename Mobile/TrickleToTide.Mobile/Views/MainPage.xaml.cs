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

            MessagingCenter.Subscribe<ILocationUpdates>(this, Constants.Message.TRACKING_STATE_CHANGED, (sender) => {
                CentreAndZoom();
            });

            MessagingCenter.Subscribe<PositionUpdate[]>(this, Constants.Message.POSITIONS_UPDATED, (positions) => {
                CentreAndZoom();
            });

            MessagingCenter.Subscribe<string>(this, Constants.Message.TARGET_UPDATED, (target) => {
                CentreAndZoom();
            });

        }


        protected override void OnAppearing()
        {
            base.OnAppearing();

            CentreAndZoom();
        }


        private void CentreAndZoom()
        {
            var source = State.Positions.AsEnumerable();

            switch (State.SelectedTarget)
            {
                case TargetOption.All:
                    if(State.Category != "Dev")
                    {
                        // Filter out Dev entities unless we're a Dev ourself
                        source = source.Where(x => x.Category != "Dev");
                    }
                    break;

                case TargetOption.Self:
                    source = source.Where(x => x.Id == State.Id);
                    break;
            }

            if (source.Any())
            {
                var maxLat = source.Max(p => p.Position.Latitude);
                var minLat = source.Min(p => p.Position.Latitude);
                var maxLon = source.Max(p => p.Position.Longitude);
                var minLon = source.Min(p => p.Position.Longitude);

                var centreLat = (minLat + maxLat) / 2;
                var centreLon = (minLon + maxLon) / 2;
                var distance = GeoCode.CalcDistance(minLat, minLon, maxLat, maxLon, GeoCodeCalcMeasurement.Kilometers);
                distance = distance < 0.5 ? 0.5 : distance;

                map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(centreLat, centreLon), Distance.FromKilometers(distance)));
            }
            else
            {
                map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(Constants.Default.LATITUDE, Constants.Default.LONGITUDE), Distance.FromKilometers(100)));
            }
        }
    }
}