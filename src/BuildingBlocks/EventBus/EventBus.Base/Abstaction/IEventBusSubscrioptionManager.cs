using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventBus.Base.Events;

namespace EventBus.Base.Abstaction
{
    public interface IEventBusSubscrioptionManager
    {

        bool IsEmpty { get; }

        event EventHandler<string> OnEventRemoved;
        void AddSubscription<T, Thandler>() where T : IntegrationEvent where Thandler : IIntegrationEventHandler<T>;
        void RemoveSubscription<T, Thandler>() where T : IntegrationEvent where Thandler : IIntegrationEventHandler<T>;

        bool HasSubscriptionForEvent<T>() where T : IntegrationEvent;
        bool HasSubscriptionForEvent(string eventName);

        Type GetEventTypeByName(string eventName);
        void Clear();
        IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent;
        IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName);

        string GetEventKey<T>();


    }
}
