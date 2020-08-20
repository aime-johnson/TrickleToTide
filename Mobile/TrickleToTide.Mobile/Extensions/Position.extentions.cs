using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms.Maps;

namespace TrickleToTide.Mobile
{
    public static class PositionExtensions
    {
        public static Position GetCentrePosition(this IEnumerable<Position> source)
        {
            var maxLat = source.Max(p => p.Latitude);
            var minLat = source.Min(p => p.Latitude);
            var maxLon = source.Max(p => p.Longitude);
            var minLon = source.Min(p => p.Longitude);

            var centreLat = (minLat + maxLat) / 2;
            var centreLon = (minLon + maxLon) / 2;

            return new Position(centreLat, centreLon);
        }

    }
}
