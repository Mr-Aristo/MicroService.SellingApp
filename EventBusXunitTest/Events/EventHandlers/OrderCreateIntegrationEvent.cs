using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventBus.Base.Events;

namespace EventBusXunitTest.Events.EventHandlers
{
    public class OrderCreateIntegrationEvent : IntegrationEvent
    {
        public int Id { get; set; }

        public OrderCreateIntegrationEvent(int id)
        {
            Id = id;
        }

    }
}
