using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Media;
namespace StatusIndicator
{
    public class WindowsStatusIndicator : IStatusIndicator
    {

        public event EventHandler NotificationClicked;
        public event EventHandler StatusClicked;

        private Status _status;
        public Status Status
        {
            get => _status;
            set
            {
                _status = value;
            }
        }


        public WindowsStatusIndicator()
        {
        }

        public void SendNotification(string title, string message)
        {
        }
    }
}