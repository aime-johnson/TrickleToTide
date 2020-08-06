using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace TrickleToTide.Mobile.Droid.Services
{
   class PlatformService : Interfaces.IPlatform
   {
        public string ApiKey=> Application.Context.Resources.GetString(Resource.String.api_key_ttt);

        public string ApiEndpoint => Application.Context.Resources.GetString(Resource.String.api_endpoint_ttt);

        public string LogDirectory
        {
            get
            {
                if (Android.OS.Environment.MediaMounted.Equals(Android.OS.Environment.ExternalStorageState))
                {
                    return Application.Context.GetExternalFilesDir(null).AbsolutePath;
                }
                else
                {
                    return null;
                }
            }
        }
   }
}