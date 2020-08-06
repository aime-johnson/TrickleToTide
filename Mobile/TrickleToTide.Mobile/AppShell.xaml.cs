using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrickleToTide.Mobile.Views;
using Xamarin.Forms;

namespace TrickleToTide.Mobile
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            BindingContext = this;
        }


        public Command SettingsCommand => new Command(async () => await Settings());
        private async Task Settings()
        {
            Shell.Current.FlyoutIsPresented = false;
            await Shell.Current.GoToAsync($"settings");
        }

        public Command LogCommand => new Command(async () => await Log());
        private async Task Log()
        {
            Shell.Current.FlyoutIsPresented = false;
            await Shell.Current.GoToAsync($"log");
        }

    }
}
