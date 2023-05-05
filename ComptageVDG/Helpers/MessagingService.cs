using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptageVDG.Helpers
{
    public class MessagingService
    {
        private static readonly object sender = new object();

        public event EventHandler<MessageEventArgs> Notify;
        public void Notification(string Sender, string Data, [System.Runtime.CompilerServices.CallerMemberName] string PropertyName = "")
        {
            Notify?.Invoke(this, new MessageEventArgs(Sender, Data,PropertyName));
        }

        public void Notification<T>(string Sender, T Data, [System.Runtime.CompilerServices.CallerMemberName] string PropertyName = "")
        {
            Notify?.Invoke(this, new MessageEventArgs(Sender, Data, PropertyName));
        }

    }


    public class MessageEventArgs
    {
        public string Sender { get; set; }
        public string PropertyName { get; set; }
        public object Data { get; set; }
        public Type? Type { get; set; }

        public MessageEventArgs(string Sender, object Data, string propertyName)
        {
            this.Data = Data;
            this.Sender = Sender;
            this.PropertyName = propertyName;
            this.Type =  Data?.GetType();
        }
    }

}
