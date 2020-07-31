using Shiny;
using Shiny.Locations;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TrickleToTide.Mobile
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            //DependencyService.Register<MockDataStore>();

            Xamarin.Essentials.Connectivity.ConnectivityChanged += ConnectivityChanged;

            MainPage = new AppShell();
        }


        private void ConnectivityChanged(object sender, Xamarin.Essentials.ConnectivityChangedEventArgs e)
        {
            MessagingCenter.Send(this, "ConnectionChanged", e);
        }

        protected async override void OnStart()
        {
            var gpsManager = ShinyHost.Resolve<IGpsManager>();

            await gpsManager.RequestAccessAndStart(new GpsRequest()
            {
                Interval = TimeSpan.FromSeconds(10),
                Priority = GpsPriority.Highest,
                UseBackground = true
            });

            MessagingCenter.Send<IGpsManager>(gpsManager, "GpsConnectionChanged");
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
