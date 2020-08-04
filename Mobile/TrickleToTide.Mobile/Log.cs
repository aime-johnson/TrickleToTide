using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using TrickleToTide.Mobile.Interfaces;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TrickleToTide.Mobile
{
    public static class Log
    {
        public static ObservableCollection<LogEvent> Events { get; } = new ObservableCollection<LogEvent>();
        public static string LogDirectory { get; private set; }

        static Log()
        {
            LogDirectory = DependencyService.Resolve<IPlatform>().LogDirectory;
        }

        public static void Event(string message)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Events.Insert(0, new LogEvent()
                {
                    Message = message
                });

                while (Events.Count() > 1000)
                {
                    Events.RemoveAt(Events.Count() - 1);
                }
            });
        }


        public static void Error(Exception ex)
        {
            Log.Event(ex.ToString());
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
