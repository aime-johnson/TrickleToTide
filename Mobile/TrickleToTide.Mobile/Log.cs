using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace TrickleToTide.Mobile
{
    static class Log
    {
        public static ObservableCollection<LogEvent> Events { get; } = new ObservableCollection<LogEvent>();
        public static void Event(string message)
        {
            Events.Insert(0, new LogEvent()
            {
                Message = message
            });

            while(Events.Count() > 1000)
            {
                Events.RemoveAt(Events.Count()-1);
            }
        }
    }


    public class LogEvent
    {
        public LogEvent()
        {
            Timestamp = DateTime.Now;
        }

        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
    }
}
