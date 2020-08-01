using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}