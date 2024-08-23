using EventBus.Base.Events;

namespace PaymentService.API.IntegrationEvents.Events
{
    public class OrderPaymentFailedIntegrationEvent :  IntegrationEvent 
    {
        public Guid OrderId { get;}
        public string ErrorMessage { get; }

        public OrderPaymentFailedIntegrationEvent(Guid orderId, string errorMessage)
        {
            OrderId = orderId;
            ErrorMessage = errorMessage;
        }


    }
}
