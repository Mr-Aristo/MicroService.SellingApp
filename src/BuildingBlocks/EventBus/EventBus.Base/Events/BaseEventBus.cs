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
        public EventBusConfig EventBusConfig { get; set; }

        protected BaseEventBus(EventBusConfig eventBusConfig, IServiceProvider serviceProvider)
        {
            this.EventBusConfig = eventBusConfig;
            ServiceProvider = serviceProvider;
            SubscrioptionManager = new InMemoryEventBusSubscriptionManager(ProcessEventName);

        }
       
        public virtual string ProcessEventName(string eventName)
        {
            if (EventBusConfig.DeleteEventPrefix)
                eventName = eventName.TrimStart(EventBusConfig.EventNamePrefix.ToArray());
            if (EventBusConfig.DeleteEventSuffix)
                eventName = eventName.TrimEnd(EventBusConfig.EventNameSuffix.ToArray());

            return eventName;
        }
        public virtual string GetSubName(string eventName)
        {
            return $"{EventBusConfig.SubscriberClientAppName}.{ProcessEventName(eventName)}";
        }
        public virtual void Dispose()
        {
            EventBusConfig = null;
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

                        var eventType = SubscrioptionManager.GetEventTypeByName($"{EventBusConfig.EventNamePrefix}{eventName}{EventBusConfig.EventNameSuffix}");
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

        //Assagidaki metodlar azure service bus yada rabbitmq tarafinda implemente edilecek.
        public virtual void Publish(IntegrationEvent @event)
        {
            throw new NotImplementedException();
        }

        public virtual void Subscribe<T, THandler>()
            where T : IntegrationEvent
            where THandler : IIntegrationEventHandler<T>
        {
            throw new NotImplementedException();
        }

        public virtual void UnSubscribe<T, THandler>()
            where T : IntegrationEvent
            where THandler : IIntegrationEventHandler<T>
        {
            throw new NotImplementedException();
        }
    }
}
