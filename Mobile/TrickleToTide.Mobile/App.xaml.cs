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

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
