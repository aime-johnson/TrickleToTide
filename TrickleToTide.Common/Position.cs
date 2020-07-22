using System;

namespace TrickleToTide.Common
{
    public class Position
    {
        /// <summary>
        /// Local time
        /// </summary>
        public DateTime Timestamp { get; set; }

        public double Lat { get; set; }
        public double Lon { get; set; }
        public double Alt { get; set; }

        /// <summary>
        /// Nickname
        /// </summary>
        public string Nick { get; set; }
    }
}
