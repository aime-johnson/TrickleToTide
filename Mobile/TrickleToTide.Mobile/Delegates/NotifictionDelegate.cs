using Shiny.Notifications;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TrickleToTide.Mobile.Delegates
{
    class NotifictionDelegate : INotificationDelegate
    {
        public Task OnEntry(NotificationResponse response)
        {
            return Task.CompletedTask;
        }

        public Task OnReceived(Notification notification)
        {
            return Task.CompletedTask;
        }
    }
}
