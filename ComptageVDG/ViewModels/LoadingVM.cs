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
            Message.Notify += Message_Notify;
        }

        private void Message_Notify(object? sender, Helpers.MessageEventArgs e)
        {
            if(e.Sender == "LOADING")
            {
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    if (e.Data is string msg && !string.IsNullOrEmpty(msg))
                        MessageLoading = msg;
                    IsLoading = true;
                });
                
            }

            if(e.Sender == "UNLOADING")
            {
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageLoading = String.Empty;
                    IsLoading = false;
                });
              
            }

        }
    }
}
