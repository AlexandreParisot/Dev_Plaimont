using ComptageVDG.Helpers;
using ComptageVDG.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace ComptageVDG.ViewModels
{
    internal class LoadingVM:BaseViewModel
    {
        private bool _loading;
        public bool IsLoading { get => _loading; set => SetProperty(ref _loading,value); }

        private string _messageLoading;
        public string MessageLoading { get => _messageLoading; set => SetProperty(ref _messageLoading, value); }

        public LoadingVM()
        {
            MessageBrokerImpl.Instance?.Subscribe<MessageEventArgs>(Payload);
        }

        private void Payload(MessagePayload<MessageEventArgs> obj)
        {
            if (obj.What.Sender == "LOADING")
            {
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    if (obj.What.Data is string msg && !string.IsNullOrEmpty(msg))
                        MessageLoading = msg;
                    IsLoading = true;
                });

            }

            if (obj.What.Sender == "UNLOADING")
            {
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageLoading = String.Empty;
                    IsLoading = false;
                });

            }
        }

        public void Close()
        {
            MessageBrokerImpl.Instance.Unsubscribe<MessageEventArgs>(Payload);  
        }
    }
}
