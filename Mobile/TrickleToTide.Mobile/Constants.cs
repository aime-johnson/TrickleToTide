using System;
using System.Collections.Generic;
using System.Text;

namespace TrickleToTide.Mobile
{
    public static class Constants
    {
        public static class Message
        {
            public const string TRACKING_STATE_CHANGED = "tracking-state-changed";
            public const string GPS_STATE_CHANGED = "gps-state-changed";
            public const string POSITIONS_UPDATED = "positions-updated";
            
            /// <summary>
            /// Device GPS location has changed
            /// </summary>
            public const string LOCATION_UPDATED = "location-updated";
            public const string CONNECTION_STATE_CHANGED = "connection-state-changed";
        }
    }
}
