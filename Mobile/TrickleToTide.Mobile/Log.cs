using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace TrickleToTide.Mobile
{
    static class Log
    {
        public static ObservableCollection<LogEvent> Events { get; } = new ObservableCollection<LogEvent>();
        public static void Event(string message)
        {
            Events.Add(new LogEvent() {
                Message = message
            });
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
