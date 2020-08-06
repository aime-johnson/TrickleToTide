using System;
using System.Collections.Generic;
using System.Text;

namespace TrickleToTide.Api.DAL
{
    class Position
    {
        public Position()
        {
            History = new List<PositionHistory>();
        }

        public Guid Id { get; set; }

        public DateTime Timestamp { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public string Category { get; set; }
        public string Nickname { get; set; }
        public double Accuracy { get; set; }
        public double Heading { get; set; }
        public double Speed { get; set; }

        public ICollection<PositionHistory> History { get; set; }
    }

    class PositionHistory
    {
        public int Id { get; set; }
        public Guid PositionId { get; set; }
        public Position Position { get; set; }
        public DateTime Timestamp { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public double Accuracy { get; set; }
        public double Heading { get; set; }
        public double Speed { get; set; }
    }
}
