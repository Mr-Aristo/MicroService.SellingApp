using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventBus.Base.Abstaction;
using EventBus.Base.SubscriptionManagers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace EventBus.Base.Events
{
    public abstract class BaseEventBus : IEventBus
    {
        public readonly IServiceProvider ServiceProvider;
        public readonly IEventBusSubscrioptionManager SubscrioptionManager;
        private EventBusConfig eventBusConfig;

        protected BaseEventBus(EventBusConfig eventBusConfig, IServiceProvider serviceProvider)
        {
            this.eventBusConfig = eventBusConfig;
            ServiceProvider = serviceProvider;
            SubscrioptionManager = new InMemoryEventBusSubscriptionManager(ProcessEventName);

        }

        public virtual string ProcessEventName(string eventName)
        {
            if (eventBusConfig.DeleteEventPrefix)
                eventName = eventName.TrimStart(eventBusConfig.EventNamePrefix.ToArray());
            if (eventBusConfig.DeleteEventSuffix)
                eventName = eventName.TrimEnd(eventBusConfig.EventNameSuffix.ToArray());

            return eventName;

        }
        public virtual string GetSubName(string eventName)
        {
            return $"{eventBusConfig.SubscriberClientAppName}.{ProcessEventName(eventName)}";
        }
        public virtual void Dispose()
        {
            eventBusConfig = null;
        }

        public async Task<bool> ProcessEvent(string eventName, string message)
        {
            eventName = ProcessEventName(eventName);
            var prosessed = false;

            if (!SubscrioptionManager.HasSubscriptionForEvent(eventName))
            {
                var subscriptions = SubscrioptionManager.GetHandlersForEvent(eventName);

                using (var scope = ServiceProvider.CreateScope())
                {
                    foreach (var subscription in subscriptions)
                    {
                        var handler = ServiceProvider.GetService(subscription.HandleType);
                        if (handler == null) continue;

                        var eventType = SubscrioptionManager.GetEventTypeByName($"{eventBusConfig.EventNamePrefix}{eventName}{eventBusConfig.EventNameSuffix}");
                        var integrationEvent = JsonConvert.DeserializeObject(message, eventType);

                        //if (integrationEvent is IntegrationEvent)
                        //{
                        //    eventBusConfig.CorrelationIdSetter?.Invoke((integrationEvent is IntegrationEvent).CorrelationId);
                        //}
                        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                        await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
                    }

                }
                prosessed = true;
            }
            return prosessed;
        }
        public void Publish(IntegrationEvent @event)
        {
            throw new NotImplementedException();
        }

        public void Subscribe<T, THandler>()
            where T : IntegrationEvent
            where THandler : IIntegrationEventHandler<T>
        {
            throw new NotImplementedException();
        }

        public void UnSubscribe<T, THandler>()
            where T : IntegrationEvent
            where THandler : IIntegrationEventHandler<T>
        {
            throw new NotImplementedException();
        }
    }
}
