using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using TrickleToTide.Mobile.Droid.Services;
using TrickleToTide.Mobile.Interfaces;
using Xamarin.Forms;

[assembly: Dependency(typeof(LocationService))]
namespace TrickleToTide.Mobile.Droid.Services
{
    class LocationService : ILocationService
    {
        public void Start()
        {
            Android.App.Application.Context.StartForegroundService<myLocationService>();
        }
    }


    [Service]
    public class myLocationService : Service
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
            intent.AddFlags(ActivityFlags.NoUserAction); // singletop
            intent.PutExtra("Title", "Message");

            var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.UpdateCurrent);

            var notifBuilder = new NotificationCompat.Builder(context, foregroundChannelId)
                .SetContentTitle("Your Title")
                .SetContentText("Main Text Body")
                .SetSmallIcon(Resource.Drawable.xamarin_logo)
                .SetOngoing(true)
                .SetContentIntent(pendingIntent);

            // Building channel if API verion is 26 or above
            if (global::Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                NotificationChannel notificationChannel = new NotificationChannel(foregroundChannelId, "Title", NotificationImportance.High);
                notificationChannel.Importance = NotificationImportance.High;
                notificationChannel.EnableLights(true);
                notificationChannel.EnableVibration(true);
                notificationChannel.SetShowBadge(true);
                notificationChannel.SetVibrationPattern(new long[] { 100, 200, 300, 400, 500, 400, 300, 200, 400 });

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