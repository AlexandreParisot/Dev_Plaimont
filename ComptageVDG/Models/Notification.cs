using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ComptageVDG.Models
{
    public class Notification
    {
        public string Message { get; set; }
        public string BgColor { get; set; }
        public string FtColor { get; set; }


        public int TimeOut { get; set; }

        private DispatcherTimer timer;

        public Notification(int timeout = 2)
        {
            TimeOut = timeout;
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(TimeOut * 10000000);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        public event RaiseNotif RaiseNotificationKill;

        public delegate void RaiseNotif(Notification Me);


        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                timer.Stop();
                timer.Tick -= Timer_Tick;
                if (RaiseNotificationKill != null)
                    RaiseNotificationKill.Invoke(this);
            }
            catch (Exception ex)
            {
                //GestionService.LogError($"Notification Erreur : {ex.Message}");
            }

        }
    }

    public class ErrorNotification : Notification
    {
        public ErrorNotification(string message, int timeout) : base(timeout)
        {
            Message = message;
            BgColor = "#F8D7DA";
            FtColor = "#932029";

        }
    }

    public class SuccessNotification : Notification
    {
        public SuccessNotification(string message, int timeout) : base(timeout)
        {
            Message = message;
            BgColor = "#D1E7DD";
            FtColor = "#0F5132";
        }
    }

    public class InfoNotification : Notification
    {
        public InfoNotification(string message, int timeout) : base(timeout)
        {
            Message = message;
            BgColor = "#CFE2FF";
            FtColor = "#0842A5";
        }
    }

    public class WarningNotification : Notification
    {
        public WarningNotification(string message, int timeout) : base(timeout)
        {
            Message = message;
            BgColor = "#FFF3CD";
            FtColor = "#CE9103";
        }
    }
}
