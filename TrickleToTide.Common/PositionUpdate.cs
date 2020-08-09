using System;

namespace TrickleToTide.Common
{
    public class PositionUpdate
    {
        public Guid Id { get; set; }

        /// <summary>
        /// UTC
        /// </summary>
        public DateTime Timestamp { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public string Category { get; set; }
        public string Nickname { get; set; }
        //public double Accuracy { get; set; }
        //public double Heading { get; set; }
        //public double Speed { get; set; }
    }
}
