using Shiny;
using Shiny.Locations;
using System;
using System.Threading;
using TrickleToTide.Mobile.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TrickleToTide.Mobile
{
    public partial class App : Application
    {
        private IGpsManager _gpsManager;

        public App()
        {
            InitializeComponent();

            //DependencyService.Register<MockDataStore>();

            Connectivity.ConnectivityChanged += ConnectivityChanged;
            DeviceDisplay.KeepScreenOn = true;

            MainPage = new AppShell();
        }


        private void ConnectivityChanged(object sender, Xamarin.Essentials.ConnectivityChangedEventArgs e)
        {
            MessagingCenter.Send(this, "ConnectionChanged", e);
        }

        protected async override void OnStart()
        {
            Log.Event("Start");
            
            _gpsManager = ShinyHost.Resolve<IGpsManager>();

            await _gpsManager.RequestAccessAndStart(new GpsRequest()
            {
                Interval = TimeSpan.FromSeconds(10),
                Priority = GpsPriority.Highest,
                UseBackground = true
            });

            MessagingCenter.Send<IGpsManager>(_gpsManager, "GpsConnectionChanged");
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
