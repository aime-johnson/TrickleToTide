using Shiny.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            var lat = Preferences.Get("ttt-lat", 51.489271);
            var lon = Preferences.Get("ttt-lon", -0.235422);
            map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(lat, lon), new Distance(100)));

            MessagingCenter.Subscribe<GpsDelegate, IGpsReading>(this, "OnReading", (sender, reading) =>
            {
                map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(reading.Position.Latitude, reading.Position.Longitude), new Distance(100)));
            });
        }
    }
}