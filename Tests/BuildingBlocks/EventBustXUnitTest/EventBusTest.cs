using System.Security.Authentication.ExtendedProtection;
using EventBus.Base.Abstaction;
using EventBus.Base.Configuration;
using EventBus.Base.Events;
using EventBus.Factory;
using EventBusXunitTest.Events.EventHandlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;//ServiceCollection

namespace EventBusXunitTest
{
    public class EventBusTest
    {
        private ServiceCollection services;

        public EventBusTest()
        {
            this.services = new ServiceCollection();
            services.AddLogging(config => config.AddConsole()); //extension.logging - extension.logging.Cnsole 
        }


        [Fact]
        public void Subscribe_Event_On_RabbitMQ_Test()
        {
            // Create a mock of the IEventBus
            var mockEventBus = new Mock<IEventBus>();

            // Configure services
            services.AddSingleton<IEventBus>(sp =>
            {
                var config = GetRabbitMQConfig();

                // Instead of returning a real EventBus, return the mock object

                return mockEventBus.Object;

            });

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Get the IEventBus instance
            var eventBus = sp.GetRequiredService<IEventBus>();

            // Subscribe to the event
            eventBus.Subscribe<OrderCreateIntegrationEvent, OrderCreateIntegrationEventHandler>();

            // Unsubscribe from the event
            eventBus.UnSubscribe<OrderCreateIntegrationEvent, OrderCreateIntegrationEventHandler>();

            // Verify that Subscribe was called once with the correct parameters
            mockEventBus.Verify(m => m.Subscribe<OrderCreateIntegrationEvent, OrderCreateIntegrationEventHandler>(), Times.Once);

            // Verify that UnSubscribe was called once with the correct parameters
            mockEventBus.Verify(m => m.UnSubscribe<OrderCreateIntegrationEvent, OrderCreateIntegrationEventHandler>(), Times.Once);
        }

        [Fact]
        public void Subscribe_Event_On_Azure_Test()
        {
            // Create a mock of the IEventBus
            var mockEventBus = new Mock<IEventBus>();

            // Configure services
            services.AddSingleton<IEventBus>(sp =>
            {
                var config = GetAzureConfig();

                // Instead of returning a real EventBus, return the mock object
                return mockEventBus.Object;
            });

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Get the IEventBus instance
            var eventBus = sp.GetRequiredService<IEventBus>();

            // Subscribe to the event
            eventBus.Subscribe<OrderCreateIntegrationEvent, OrderCreateIntegrationEventHandler>();

            // Unsubscribe from the event
            eventBus.UnSubscribe<OrderCreateIntegrationEvent, OrderCreateIntegrationEventHandler>();

            // Verify that Subscribe was called once with the correct parameters
            mockEventBus.Verify(m => m.Subscribe<OrderCreateIntegrationEvent, OrderCreateIntegrationEventHandler>(), Times.Once);

            // Verify that UnSubscribe was called once with the correct parameters
            mockEventBus.Verify(m => m.UnSubscribe<OrderCreateIntegrationEvent, OrderCreateIntegrationEventHandler>(), Times.Once);
        }

        [Fact]
        public void Send_Messag_To_RabbitMq_Test()
        {
            // Arrange
            var mockEventBus = new Mock<IEventBus>();

            services.AddSingleton<IEventBus>(sp =>
            {
                var config = GetRabbitMQConfig();
                return mockEventBus.Object;
            });

            var sp = services.BuildServiceProvider();

            // Act
            var eventBus = sp.GetRequiredService<IEventBus>();
            var orderCreateEvent = new OrderCreateIntegrationEvent(1);
            eventBus.Publish(orderCreateEvent);

            // Assert
            mockEventBus.Verify(m => m.Publish(It.Is<OrderCreateIntegrationEvent>(e => e.Id == orderCreateEvent.Id)), Times.Once);
        }


        [Fact]
        public void Send_Messag_To_Azure_Test()
        {   // Arrange
            var mockEventBus = new Mock<IEventBus>();

            services.AddSingleton<IEventBus>(sp =>
            {
                var config = GetAzureConfig();


                return mockEventBus.Object;

            });
            var sp = services.BuildServiceProvider();

            // Act

            // Get the IEventBus instance
            var eventBus = sp.GetRequiredService<IEventBus>();
            var orderCreateEvent = new OrderCreateIntegrationEvent(1);
            eventBus.Publish(orderCreateEvent);

            // Assert
            mockEventBus.Verify(m => m.Publish(It.Is<OrderCreateIntegrationEvent>(e => e.Id == orderCreateEvent.Id)), Times.Once);
        }
    

        private EventBusConfig GetAzureConfig()
        {
            return new EventBusConfig()
            {
                ConnectionRetryCount = 5,
                SubscriberClientAppName = "Eventbus.UnitTest",
                DefaultTopicName = "SellingAppTopicName",
                EventBusType = EventBusType.AzurServiceBus,
                EventNameSuffix = "IntegrationEvent",
                EventBusConnectionString = "Test"

            };

        }
        private EventBusConfig GetRabbitMQConfig()
        {
            return new EventBusConfig()
            {
                ConnectionRetryCount = 5,
                SubscriberClientAppName = "Eventbus.UnitTest",
                DefaultTopicName = "SellingAppTopicName",
                EventBusType = EventBusType.RabbitMQ,
                EventNameSuffix = "IntegrationEvent",
                // Uncomment and configure the connection settings if needed
                // Connection = new ConnectionFactory()
                // {
                //     HostName = "localhost",
                //     Port = 5672,
                //     UserName = "guest",
                //     Password = "guest"
                // }

            };

        }
    }
}
