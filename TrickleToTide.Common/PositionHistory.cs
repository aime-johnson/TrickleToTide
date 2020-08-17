using System;

namespace TrickleToTide.Common
{
    public class PositionHistory
    {
        public DateTime Timestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
    }
}
