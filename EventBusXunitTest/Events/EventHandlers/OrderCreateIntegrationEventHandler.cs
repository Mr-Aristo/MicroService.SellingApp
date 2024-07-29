﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventBus.Base.Abstaction;

namespace EventBusXunitTest.Events.EventHandlers
{
    public class OrderCreateIntegrationEventHandler : IIntegrationEventHandler<OrderCreateIntegrationEvent>
    {
        public Task Handle(OrderCreateIntegrationEvent @event)
        {
            return Task.CompletedTask;
        }
    }
}
