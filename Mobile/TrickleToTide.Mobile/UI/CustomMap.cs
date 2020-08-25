using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Maps;

namespace TrickleToTide.Mobile.UI
{
    public class CustomMap : Map
    {
        public List<CustomPin> CustomPins { get; set; }
    }

    public class CustomPin : Pin
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
}
