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

            map.MapElements.Clear();
            map.MapElements.Add(Constants.Default.ROUTE);

            CentreAndZoom();
        }


        private void CentreAndZoom()
        {
            if(State.SelectedTarget != TargetOption.None)
            {
                var source = State.Positions.AsEnumerable();


                switch (State.SelectedTarget)
                {
                    case TargetOption.All:
                        break;

                    case TargetOption.Self:
                        source = source.Where(x => x.Id == State.Id);
                        break;

                    case TargetOption.SelfAndSelected:
                        source = source.Where(x => x.Id == State.Id || x.Id == State.SelectedId);
                        break;

                    case TargetOption.Selected:
                        source = source.Where(x => x.Id == State.SelectedId);
                        break;
                }
                bool displayRoute = false;
                // Default to the route if we have nothing better
                if (!source.Any())
                {
                    Log.Event("No Source positions: Displaying route");

                    source = Constants.Default.ROUTE.Geopath.Select(x => new PositionViewModel() {
                        Position = x
                    });

                    displayRoute = true;
                }

                var maxLat = source.Max(p => p.Position.Latitude);
                var minLat = source.Min(p => p.Position.Latitude);
                var maxLon = source.Max(p => p.Position.Longitude);
                var minLon = source.Min(p => p.Position.Longitude);

                var distance = GeoCode.CalcDistance(minLat, minLon, maxLat, maxLon, GeoCodeCalcMeasurement.Kilometers);
                distance = distance < 0.5 ? 0.5 : distance;
                if (displayRoute)
                {
                    distance /=  1.8;
                }

                map.MoveToRegion(MapSpan.FromCenterAndRadius(source.Select(p => p.Position).GetCentrePosition(), Distance.FromKilometers(distance)));
            }
        }


        // Route pin clicks back to VM (Select position)
        private void MarkerClicked(object sender, PinClickedEventArgs e)
        {
            var pin = sender as Pin;
            if(pin != null)
            {
                var x = pin.BindingContext as PositionViewModel;
                var vm = BindingContext as MainViewModel;
                if (x != null && vm != null)
                {
                    vm.SelectedPosition = x;
                }
            }
        }


        // Rout map clicks back to VM (Un-Select position)
        private void MapClicked(object sender, MapClickedEventArgs e)
        {
            var vm = BindingContext as MainViewModel;
            if (vm != null)
            {
                vm.SelectedPosition = null;
            }
        }
    }
}