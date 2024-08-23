using EventBus.Base.Events;

namespace PaymentService.API.IntegrationEvents.Events
{
    public class OrderPaymentSuccessIntegrationEvent : IntegrationEvent
    {

        public Guid OrderId { get; }

        public OrderPaymentSuccessIntegrationEvent(Guid orderId) => OrderId = orderId; 
        
    }
}
