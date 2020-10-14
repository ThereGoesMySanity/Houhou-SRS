
using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;
namespace StatusIndicator
{
    public enum Status { IDLE, ACTIVE }
    public interface IStatusIndicator
    {
        event EventHandler NotificationClicked;

        event EventHandler StatusClicked;

        Status Status { get; set; }
        
        void SendNotification(string title, string message);
    }
    public class StatusIndicator : IStatusIndicator
    {
        public static IStatusIndicator Get()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new LinuxStatusIndicator();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsStatusIndicator();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                //TODO?
            }
            return new StatusIndicator();
        }
        public event EventHandler NotificationClicked;

        public event EventHandler StatusClicked;

        public Status Status { get; set; }
        
        public void SendNotification(string title, string message) {}
    }
}