using Shiny;
using Shiny.Locations;
using System;
using System.Threading;
using System.Threading.Tasks;
using TrickleToTide.Mobile.Interfaces;
using TrickleToTide.Mobile.Services;
using TrickleToTide.Mobile.Views;
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

            Routing.RegisterRoute("settings", typeof(SettingsPage));
            Routing.RegisterRoute("log", typeof(LogPage));
        }


        private void ConnectivityChanged(object sender, Xamarin.Essentials.ConnectivityChangedEventArgs e)
        {
            MessagingCenter.Send(this, Constants.Message.CONNECTION_STATE_CHANGED, e);
        }


        protected override void OnStart()
        {
            Log.Event("Start");            
            _locationUpdates.StartGps();
            State.RefreshPositions();
        }


        protected override void OnSleep()
        {
            Log.Event("Sleep");
        }

        protected override void OnResume()
        {
            Log.Event("Resume");
            State.RefreshPositions();
        }
    }
}
