using EventBus.Base.Abstaction;
using EventBus.Base.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentService.API.IntegrationEvents.Events;
using System.Threading.Tasks;

namespace PaymentService.Api.IntegrationEvents.EventHandlers
{
    //When IntegrationEvent sent,PaymentService will run. 
    public class OrderStartedIntegrationEventHandler : IIntegrationEventHandler<OrderStartedIntegrationEvent>
    {
        private readonly IConfiguration configuration;
        private readonly IEventBus eventBus;
        private readonly ILogger<OrderStartedIntegrationEventHandler> logger;

        public OrderStartedIntegrationEventHandler(IConfiguration configuration, IEventBus eventBus, ILogger<OrderStartedIntegrationEventHandler> logger)
        {
            this.configuration = configuration;
            this.eventBus = eventBus;
            this.logger = logger;
        }

        //BaseEventBus icindeki  .GetMethod("Handle").Invoke ... 
        //Odeme islemini burada handle ediyoruz.
        public Task Handle(OrderStartedIntegrationEvent @event)
        {
            // Fake payment process
            string keyword = "PaymentSuccess";
            bool paymentSuccessFlag = configuration.GetValue<bool>(keyword);

            IntegrationEvent paymentEvent = paymentSuccessFlag
                ? new OrderPaymentSuccessIntegrationEvent(@event.OrderId)
                : new OrderPaymentFailedIntegrationEvent(@event.OrderId, "This is a fake error message");

            logger.LogInformation($"OrderStartedIntegrationEventHandler in PaymentService is fired with PaymentSuccess: {paymentSuccessFlag}, orderId: {@event.OrderId}");

            eventBus.Publish(paymentEvent);

            return Task.CompletedTask;
        }
    }
}
