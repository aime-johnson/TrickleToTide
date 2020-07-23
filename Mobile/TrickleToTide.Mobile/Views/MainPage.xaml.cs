using Shiny.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrickleToTide.Mobile.Delegates;
using TrickleToTide.Mobile.ViewModels;
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

            this.BindingContext = new MainViewModel();

            MessagingCenter.Subscribe<GpsDelegate, IGpsReading>(this, "OnReading", (sender, reading) =>
            {
                map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(reading.Position.Latitude, reading.Position.Longitude), new Distance(100)));
            });
        }
    }
}