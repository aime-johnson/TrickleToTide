using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Shiny;
using Shiny.Locations;
using TrickleToTide.Mobile.Droid.Services;
using TrickleToTide.Mobile.Interfaces;
using TrickleToTide.Mobile.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(LocationUpdateService))]
namespace TrickleToTide.Mobile.Droid.Services
{
    class LocationUpdateService : ILocationUpdates
    {
        private readonly IGpsManager _gpsManager;
        private bool _gpsAvailable = false;

        public LocationUpdateService()
        {
            _gpsManager = ShinyHost.Resolve<IGpsManager>();
        }


        public async void StartGps()
        {
            if (!IsGpsConnected)
            {
                var access = await _gpsManager.RequestAccessAndStart(new GpsRequest()
                {
                    Interval = TimeSpan.FromSeconds(60),
                    Priority = GpsPriority.Normal,
                    UseBackground = true
                });

                if (access != AccessState.Available)
                {
                    Log.Event($"Failed to connect GPS: {access}");
                }
                else
                {
                    _gpsAvailable = true;
                    Log.Event($"GPS Connected");
                    MessagingCenter.Send<IGpsManager>(_gpsManager, Constants.Message.GPS_STATE_CHANGED);
                }
            }
        }


        public async void StopGps()
        {
            if (IsGpsConnected)
            {
                await _gpsManager.StopListener();
                Log.Event($"GPS Disconnected");
                MessagingCenter.Send<IGpsManager>(_gpsManager, Constants.Message.GPS_STATE_CHANGED);
            }
        }

        public bool IsGpsConnected => _gpsAvailable && _gpsManager.IsListening;

        public void Start()
        {
            Android.App.Application.Context.StartForegroundService<KeepAliveService>();
            IsRunning = true;
            MessagingCenter.Send<ILocationUpdates>(this, Constants.Message.TRACKING_STATE_CHANGED);
            Log.Event("Start Tracking");
            State.RefreshPositions();
        }


        public void Stop()
        {
            Android.App.Application.Context.StopService<KeepAliveService>();
            IsRunning = false;
            MessagingCenter.Send<ILocationUpdates>(this, Constants.Message.TRACKING_STATE_CHANGED);
            Log.Event("Stop Tracking");
        }

        public bool IsRunning { get; private set; }
    }



    [Service]
    public class KeepAliveService : Service
    {
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public const int ServiceRunningNotifID = 10001;
        private const string foregroundChannelId = "9001";

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            var v = global::Android.OS.Build.VERSION.SdkInt;
            Log.Event($"[{this.GetType().Name}] OnStartCommand (DROID VERSION {v})");

            StartForeground(ServiceRunningNotifID, CreateNotification());

            //_ = DoLongRunningOperationThings();

            return StartCommandResult.Sticky;
        }


        public override void OnDestroy()
        {
            Log.Event($"[{this.GetType().Name}] OnDestroy");
            base.OnDestroy();
        }

        public override void OnLowMemory()
        {
            Log.Event($"[{this.GetType().Name}] OnLowMemory");
            base.OnLowMemory();
        }

        public override void OnTrimMemory([GeneratedEnum] TrimMemory level)
        {
            Log.Event($"[{this.GetType().Name}] OnTrimMemory");
            base.OnTrimMemory(level);
        }

        private Notification CreateNotification()
        {
            var context = Android.App.Application.Context;

            // Building intent
            var intent = new Intent(context, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.SingleTop);
            intent.PutExtra("Title", "Message");

            var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.UpdateCurrent);

            var notifBuilder = new NotificationCompat.Builder(context, foregroundChannelId)
                .SetContentTitle("Tracking Position")
                //.SetContentText("")
                .SetSmallIcon(Resource.Drawable.kayak_dude_small)
                .SetOngoing(true)
                .SetContentIntent(pendingIntent);

            // Building channel if API verion is 26 or above
            if (global::Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                NotificationChannel notificationChannel = new NotificationChannel(foregroundChannelId, "Title", NotificationImportance.None);
                notificationChannel.Importance = NotificationImportance.None;
                notificationChannel.SetShowBadge(true);

                var notifManager = context.GetSystemService(Context.NotificationService) as NotificationManager;
                if (notifManager != null)
                {
                    notifBuilder.SetChannelId(foregroundChannelId);
                    notifManager.CreateNotificationChannel(notificationChannel);
                }
            }

            return notifBuilder.Build();
        }
    }    
}