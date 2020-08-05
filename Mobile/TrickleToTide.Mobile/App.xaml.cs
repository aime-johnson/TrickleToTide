using Shiny;
using Shiny.Locations;
using System;
using System.Threading;
using System.Threading.Tasks;
using TrickleToTide.Mobile.Interfaces;
using TrickleToTide.Mobile.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TrickleToTide.Mobile
{
    public partial class App : Application
    {
        private readonly ILocationUpdates _locationUpdates;

        public App()
        {
            InitializeComponent();

            _locationUpdates = DependencyService.Resolve<ILocationUpdates>();

            Connectivity.ConnectivityChanged += ConnectivityChanged;
            DeviceDisplay.KeepScreenOn = true;

            MainPage = new AppShell();
        }


        private void ConnectivityChanged(object sender, Xamarin.Essentials.ConnectivityChangedEventArgs e)
        {
            MessagingCenter.Send(this, "ConnectionChanged", e);
        }


        protected override void OnStart()
        {
            Log.Event("Start");            
            _locationUpdates.StartGps();
        }


        protected override void OnSleep()
        {
            Log.Event("Sleep");
        }

        protected override void OnResume()
        {
            Log.Event("Resume");
        }
    }
}
