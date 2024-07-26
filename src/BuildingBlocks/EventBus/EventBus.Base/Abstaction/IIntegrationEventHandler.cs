using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventBus.Base.Events;

namespace EventBus.Base.Abstaction
{
    public interface IIntegrationEventHandler<TIntegreationEvent>:IntegrationEventHandler where TIntegreationEvent: IntegrationEvent
    {
        Task Hadle(TIntegreationEvent @event);
    }
    public interface IntegrationEventHandler
    {

    }

}
