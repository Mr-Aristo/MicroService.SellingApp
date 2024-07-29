using EventBus.AzureServiceBus;
using EventBus.Base.Abstaction;
using EventBus.Base.Configuration;
using EventBus.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Factory
{
    public static class EventBusFactory
    {
        public static IEventBus Create(EventBusConfig eventBusConfig, IServiceProvider serviceProvider)
        {
            return eventBusConfig.EventBusType switch
            {
                EventBusType.AzurServiceBus => new EventBusServiceBus(eventBusConfig, serviceProvider),
                _ => new EventBusRabbitMQ(eventBusConfig, serviceProvider)
            };
           
        }
    }
}
