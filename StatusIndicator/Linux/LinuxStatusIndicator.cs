using Notifications;
using System;

namespace StatusIndicator
{
    public class LinuxStatusIndicator : IStatusIndicator
    {
        public event EventHandler NotificationClicked;

        public event EventHandler StatusClicked;

        public Status Status { get; set; }
        
        public void SendNotification(string title, string message)
        {
            var notification = new Notification(title, message);
            notification.AddAction("review", "Start Reviewing", (o, a) => NotificationClicked?.Invoke(o, a));
        }
    }
}