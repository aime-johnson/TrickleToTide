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
        public string ApiKey => "xxxxxxxxx";

        public string ApiEndpoint => "https://swampnet-ttt.azurewebsites.net";
    }
}