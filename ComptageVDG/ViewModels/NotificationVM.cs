using ComptageVDG.Helpers;
using ComptageVDG.Helpers.Interfaces;
using ComptageVDG.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ComptageVDG.ViewModels
{
    public class NotificationVM:BaseViewModel
    {
        public ObservableCollection<Notification> Notifications { get; set; } = new ObservableCollection<Notification>();
        

        public NotificationVM()
        {
            MessageBrokerImpl.Instance.Subscribe<MessageEventArgs>(Payload);
        }

        private void Payload(MessagePayload<MessageEventArgs> obj)
        {
            if (obj.What.Sender == "ERRORNOTIF")
            {
                if (obj.What.Data is string msg && !string.IsNullOrEmpty(msg))
                    ErrorNotif(msg);
            }

            if (obj.What.Sender == "SUCCESSNOTIF")
            {
                if (obj.What.Data is string msg && !string.IsNullOrEmpty(msg))
                    SuccessNotif(msg);
            }

            if (obj.What.Sender == "WARNINGNOTIF")
            {
                if (obj.What.Data is string msg && !string.IsNullOrEmpty(msg))
                    WarningNotif(msg);
            }

            if (obj.What.Sender == "INFONOTIF")
            {
                if (obj.What.Data is string msg && !string.IsNullOrEmpty(msg))
                    InfoNotif(msg);
            }
        }

        private void ErrorNotif(string message, int second = 5)
        {

            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                ErrorNotification notif = new ErrorNotification(message, second);
                notif.RaiseNotificationKill += KillNotif;

                if (Notifications.Count() > 5)
                {
                    Notifications.Last().RaiseNotificationKill -= KillNotif;
                    Notifications.Remove(Notifications.Last());
                }
                Notifications.Add(notif); RaisPropertyChanged(nameof(Notifications));
            }));
            // Dispatcher.CurrentDispatcher.Invoke(new Action(() => { Notifications.Add(notif); RaisPropertyChanged(nameof(Notifications)); }));
            //Notifications.Add(notif); RaisPropertyChanged(nameof(Notifications));
        }

        private void SuccessNotif(string message, int second = 5)
        {

            // Notifications.Add(notif);
            // RaisPropertyChanged(nameof(Notifications));
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                SuccessNotification notif = new SuccessNotification(message, second);
                notif.RaiseNotificationKill += KillNotif;
                if (Notifications.Count() > 5)
                {
                    Notifications.Last().RaiseNotificationKill -= KillNotif;
                    Notifications.Remove(Notifications.Last());
                }
                Notifications.Add(notif); RaisPropertyChanged(nameof(Notifications));
            }));
            // Dispatcher.CurrentDispatcher.Invoke(new Action(new Action (() => { Notifications.Add(notif); RaisPropertyChanged(nameof(Notifications)); })));

        }

        private void WarningNotif(string message, int second = 5)
        {

            //Dispatcher.CurrentDispatcher.Invoke(new Action(() => { 
            //    Notifications.Add(notif);
            //    RaisPropertyChanged(nameof(Notifications));
            //}));
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                WarningNotification notif = new WarningNotification(message, second);
                notif.RaiseNotificationKill += KillNotif;
                if (Notifications.Count() > 5)
                {
                    Notifications.Last().RaiseNotificationKill -= KillNotif;
                    Notifications.Remove(Notifications.Last());
                }
                Notifications.Add(notif); RaisPropertyChanged(nameof(Notifications));
            }));
            //AddOnUI(Notifications, notif);
            //Notifications.Add(notif);
            //RaisPropertyChanged(nameof(Notifications));
        }

        private void InfoNotif(string message, int second = 5)
        {

            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                InfoNotification notif = new InfoNotification(message, second);
                notif.RaiseNotificationKill += KillNotif;
                if (Notifications.Count() > 5)
                {
                    Notifications.Last().RaiseNotificationKill -= KillNotif;
                    Notifications.Remove(Notifications.Last());
                }

                Notifications.Add(notif); RaisPropertyChanged(nameof(Notifications));
            }
            ));
            // AddOnUI(Notifications, notif);
            //Notifications.Add(notif); RaisPropertyChanged(nameof(Notifications));
        }

        private void KillNotif(Notification Me)
        {
            Me.RaiseNotificationKill -= KillNotif;
            if (Notifications.Contains(Me))
            {
                Notifications.Remove(Me);
                RaisPropertyChanged(nameof(Notifications));
            }
        }
    }
}
