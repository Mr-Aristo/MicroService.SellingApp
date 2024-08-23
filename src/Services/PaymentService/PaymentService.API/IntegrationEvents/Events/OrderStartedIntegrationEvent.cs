using EventBus.Base.Events;

namespace PaymentService.API.IntegrationEvents.Events
{
    //Which order started. 
    public class OrderStartedIntegrationEvent : IntegrationEvent
    {
        public Guid OrderId { get; set; }

        public OrderStartedIntegrationEvent()
        {

        }

        public OrderStartedIntegrationEvent(Guid orderId)
        {
            OrderId = orderId;
        }
    }
}

