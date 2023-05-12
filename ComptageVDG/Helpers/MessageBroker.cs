using ComptageVDG.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ComptageVDG.Helpers
{
    public class MessageBrokerImpl : IMessageBroker
    {
        private static MessageBrokerImpl _instance;
        private readonly Dictionary<Type, List<Delegate>> _subscribers;
        public static MessageBrokerImpl Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MessageBrokerImpl();
                return _instance;
            }
        }

        private MessageBrokerImpl()
        {
            _subscribers = new Dictionary<Type, List<Delegate>>();
        }

        public void Publish<T>(object source, T message)
        {
            if (message == null || source == null)
                return;
            if (!_subscribers.ContainsKey(typeof(T)))
            {
                return;
            }
            var delegates = _subscribers[typeof(T)];
            if (delegates == null || delegates.Count == 0) return;
            var payload = new MessagePayload<T>(message, source);
            foreach (var handler in delegates.Select
            (item => item as Action<MessagePayload<T>>))
            {
                Dispatcher.CurrentDispatcher.Invoke(() => handler?.Invoke(payload));
                //Task.Factory.StartNew(() => handler?.Invoke(payload));
            }
        }

        public void Subscribe<T>(Action<MessagePayload<T>> subscription)
        {
            var delegates = _subscribers.ContainsKey(typeof(T)) ?
                            _subscribers[typeof(T)] : new List<Delegate>();
            if (!delegates.Contains(subscription))
            {
                delegates.Add(subscription);
            }
            _subscribers[typeof(T)] = delegates;
        }

        public void Unsubscribe<T>(Action<MessagePayload<T>> subscription)
        {
            if (!_subscribers.ContainsKey(typeof(T))) return;
            var delegates = _subscribers[typeof(T)];
            if (delegates.Contains(subscription))
                delegates.Remove(subscription);
            if (delegates.Count == 0)
                _subscribers.Remove(typeof(T));
        }

        public static MessageEventArgs Notification(string Sender, object Data, [System.Runtime.CompilerServices.CallerMemberName] string PropertyName = "")
        {
            return new MessageEventArgs(Sender, Data, PropertyName);
        }

        public void Dispose()
        {
            _subscribers?.Clear();
        }
    }
}
