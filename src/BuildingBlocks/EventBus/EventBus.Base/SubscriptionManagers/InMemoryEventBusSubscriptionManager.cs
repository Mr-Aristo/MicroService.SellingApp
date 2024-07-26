using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventBus.Base.Abstaction;
using EventBus.Base.Events;

namespace EventBus.Base.SubscriptionManagers
{
    public class InMemoryEventBusSubscriptionManager : IEventBusSubscrioptionManager
    {
        private readonly Dictionary<string, List<SubscriptionInfo>> _handlers;
        private readonly List<Type> _eventTypes;

        public event EventHandler<string> OnEventRemove;
        public Func<string, string> eventNameGetter;
        public InMemoryEventBusSubscriptionManager(Func<string, string> eventNameGatter)
        {
            _handlers = new Dictionary<string, List<SubscriptionInfo>>();
            _eventTypes = new List<Type>();
            this.eventNameGetter = eventNameGatter;
        }

        public bool IsEmpty => !_handlers.Keys.Any();
        public void Clear() => _handlers.Clear();

        public event EventHandler<string> OnEventRemoved;

        public void AddSubscription<T, Thandler>()
            where T : IntegrationEvent
            where Thandler : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();
            AddSubscription(typeof(Thandler), eventName);

            if (!_eventTypes.Contains(typeof(Thandler)))
            {
                _eventTypes.Add(typeof(T));

            }

        }
        private void AddSubscription(Type handleType, string eventName)
        {
            if (!HasSubscriptionForEvent(eventName))
            {
                _handlers.Add(eventName, new List<SubscriptionInfo>());
            }

            if (_handlers[eventName].Any(s => s.HandleType == handleType))
            {
                throw new ArgumentNullException($" Hadler Type {handleType.Name} already register for '{eventName}'", nameof(handleType));
            }
            _handlers[eventName].Add(SubscriptionInfo.Typed(handleType));
        }

        private SubscriptionInfo FindSubscriptionToRemove<T, Thandler>() where T : IntegrationEvent where Thandler : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();
            return FindSubscriptionToRemove(eventName, typeof(Thandler));
        }

        private SubscriptionInfo FindSubscriptionToRemove(string eventName, Type hadlerType)
        {
            if (!HasSubscriptionForEvent(eventName))
            {
                return null;

            }
            return _handlers[eventName].SingleOrDefault(s => s.HandleType == hadlerType);

        }

        public string GetEventKey<T>()
        {
            string eventName = typeof(T).Name;
            return eventNameGetter(eventName);
        }

        public Type GetEventTypeByName(string eventName) => _eventTypes.SingleOrDefault(s => s.Name == eventName);



        public IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent
        {
            var key = GetEventKey<T>();
            return GetHandlersForEvent(key);
        }

        public IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName) => _handlers[eventName];
       

        public bool HasSubscriptionForEvent<T>() where T : IntegrationEvent
        {
            var key = GetEventKey<T>();
            return HasSubscriptionForEvent(key);
        }

        public bool HasSubscriptionForEvent(string eventName) => _handlers.ContainsKey(eventName);


        public void RemoveSubscription<T, Thandler>()
            where T : IntegrationEvent
            where Thandler : IIntegrationEventHandler<T>
        {
            throw new NotImplementedException();
        }
    }
}
