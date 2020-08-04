using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Shiny;
using Shiny.Locations;
using TrickleToTide.Common;
using TrickleToTide.Mobile.Delegates;
using TrickleToTide.Mobile.Droid.Services;
using TrickleToTide.Mobile.Interfaces;
using TrickleToTide.Mobile.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(LocationUpdateService))]
namespace TrickleToTide.Mobile.Droid.Services
{
    class LocationUpdateService : ILocationUpdates
    {
        private readonly Guid _id;
        private readonly IGpsManager _gpsManager;

        public LocationUpdateService()
        {
            _id = Guid.Parse(Preferences.Get("ttt-id", Guid.Empty.ToString()));
            _gpsManager = ShinyHost.Resolve<IGpsManager>();

            StartGps();

            MessagingCenter.Send<IGpsManager>(_gpsManager, "GpsConnectionChanged");

            MessagingCenter.Subscribe<GpsDelegate, IGpsReading>(this, "OnReading", async (sender, reading) =>
            {
                if (IsRunning)
                {
                    try
                    {
                        await Api.UpdatePositionAsync(new PositionUpdate()
                        {
                            Id = _id,
                            Latitude = reading.Position.Latitude,
                            Longitude = reading.Position.Longitude,
                            Altitude = reading.Altitude,
                            Accuracy = reading.PositionAccuracy,
                            Heading = reading.Heading,
                            Speed = reading.Speed,
                            Timestamp = reading.Timestamp,
                            Nickname = Preferences.Get("ttt-nick", "")
                        });
                    }
                    catch (Exception ex)
                    {
                        Log.Event(ex.Message);
                    }
                }

                Preferences.Set("ttt-lat", reading.Position.Latitude);
                Preferences.Set("ttt-lon", reading.Position.Longitude);
            });
        }


        public async void StartGps()
        {
            if (!IsGpsConnected)
            {
                var access = await _gpsManager.RequestAccessAndStart(new GpsRequest()
                {
                    Interval = TimeSpan.FromSeconds(10),
                    Priority = GpsPriority.Highest,
                    UseBackground = true
                });

                if (access != AccessState.Available)
                {
                    Log.Event($"Failed to connect GPS: {access}");
                }
                else
                {
                    Log.Event($"GPS Connected");
                    MessagingCenter.Send<IGpsManager>(_gpsManager, "GpsConnectionChanged");
                }
            }
        }


        public async void StopGps()
        {
            if (IsGpsConnected)
            {
                await _gpsManager.StopListener();
                Log.Event($"GPS Disconnected");
                MessagingCenter.Send<IGpsManager>(_gpsManager, "GpsConnectionChanged");
            }
        }

        public bool IsGpsConnected => _gpsManager.IsListening;

        public void Start()
        {
            Android.App.Application.Context.StartForegroundService<KeepAliveService>();
            IsRunning = true;
            MessagingCenter.Send<ILocationUpdates>(this, "fg");
            Log.Event("Start Tracking");
        }


        public void Stop()
        {
            Android.App.Application.Context.StopService<KeepAliveService>();
            IsRunning = false;
            MessagingCenter.Send<ILocationUpdates>(this, "fg");
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
            StartForeground(ServiceRunningNotifID, CreateNotification());

            //_ = DoLongRunningOperationThings();

            return StartCommandResult.Sticky;
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
                NotificationChannel notificationChannel = new NotificationChannel(foregroundChannelId, "Title", NotificationImportance.High);
                notificationChannel.Importance = NotificationImportance.High;
                //notificationChannel.EnableLights(true);
                //notificationChannel.EnableVibration(true);
                notificationChannel.SetShowBadge(true);
                notificationChannel.SetVibrationPattern(new long[] { 100, 100 });

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