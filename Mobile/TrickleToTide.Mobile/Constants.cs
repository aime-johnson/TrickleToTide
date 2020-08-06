using System;
using System.Collections.Generic;
using System.Text;

namespace TrickleToTide.Mobile
{
    public static class Constants
    {
        public static class Default
        {
            public const double LATITUDE = 51.6875751;
            public const double LONGITUDE = -2.0222528;
            public const string CATEGORY = "Splashy Boi";
            public static readonly string[] CATEGORIES = new[] { 
                CATEGORY,
                "Support",
                "Dev"
            };
        }


        public static class Message
        {
            public const string TRACKING_STATE_CHANGED = "tracking-state-changed";

            public const string GPS_STATE_CHANGED = "gps-state-changed";

            /// <summary>
            /// Positions of other trackers updated
            /// </summary>
            public const string POSITIONS_UPDATED = "positions-updated";
            
            /// <summary>
            /// Device GPS location has changed
            /// </summary>
            public const string LOCATION_UPDATED = "location-updated";

            public const string CONNECTION_STATE_CHANGED = "connection-state-changed";
        }


        public static class Preferences
        {
            public const string CATEGORY = "ttt-category";
            public const string NICKNAME = "ttt-nick";
            public const string LAST_LATITUDE = "ttt-lat";
            public const string LAST_LONGITUDE = "ttt-lon";
            public const string ID = "ttt-id";
        }
    }
}
