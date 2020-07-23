using Shiny;
using Shiny.Locations;
using Shiny.Notifications;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TrickleToTide.Mobile.Delegates;
using Xamarin.Forms;

namespace TrickleToTide.Mobile.ViewModels
{
    class MainViewModel : BaseViewModel
    {
        private readonly IGpsManager _gpsManager;
        private readonly INotificationManager _notificationManager;
        private readonly IMotionActivityManager _motionActivityManager;

        public MainViewModel()
        {
            _gpsManager = ShinyHost.Resolve<IGpsManager>();
            _notificationManager = ShinyHost.Resolve<INotificationManager>();
            _motionActivityManager = ShinyHost.Resolve<IMotionActivityManager>();

            MessagingCenter.Subscribe<GpsDelegate, IGpsReading>(this, "OnReading", (sender, reading) =>
            {
            });
        }

        public bool IsTracking => _gpsManager.IsListening;
    }
}
