using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
        private Guid _id;
        private readonly IGpsManager _gpsManager;
        private bool _xxx = false;

        public LocationUpdateService()
        {
            _id = Guid.Parse(Preferences.Get(Constants.Preferences.ID, Guid.Empty.ToString()));
            _gpsManager = ShinyHost.Resolve<IGpsManager>();

            MessagingCenter.Subscribe<GpsDelegate, IGpsReading>(this, Constants.Message.LOCATION_UPDATED, async (sender, reading) =>
            {
                if (IsRunning)
                {
                    try
                    {
                        var positions = await Api.UpdatePositionAsync(new PositionUpdate()
                        {
                            Id = Id,
                            Latitude = reading.Position.Latitude,
                            Longitude = reading.Position.Longitude,
                            Altitude = reading.Altitude,
                            Accuracy = reading.PositionAccuracy,
                            Heading = reading.Heading,
                            Speed = reading.Speed,
                            Timestamp = reading.Timestamp,
                            Nickname = Preferences.Get(Constants.Preferences.NICKNAME, "")
                        });

                        if(positions != null)
                        {
                            MessagingCenter.Send<PositionUpdate[]>(positions, Constants.Message.POSITIONS_UPDATED);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Event(ex.Message);
                    }
                }

                Preferences.Set(Constants.Preferences.LAST_LATITUDE, reading.Position.Latitude);
                Preferences.Set(Constants.Preferences.LAST_LONGITUDE, reading.Position.Longitude);
            });
        }

        public Guid Id
        {
            get
            {
                if (_id == default(Guid))
                {
                    if (!Preferences.ContainsKey(Constants.Preferences.ID))
                    {
                        _id = Guid.NewGuid();
                        Preferences.Set(Constants.Preferences.ID, _id.ToString());
                    }
                    else
                    {
                        _id = Guid.Parse(Preferences.Get(Constants.Preferences.ID, Guid.Empty.ToString()));
                    }
                }
                return _id;
            }
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
                    _xxx = true;
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

        public bool IsGpsConnected => _xxx && _gpsManager.IsListening;

        public void Start()
        {
            Android.App.Application.Context.StartForegroundService<KeepAliveService>();
            Api.ResetThrottle();
            IsRunning = true;
            MessagingCenter.Send<ILocationUpdates>(this, Constants.Message.TRACKING_STATE_CHANGED);
            Log.Event("Start Tracking");
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