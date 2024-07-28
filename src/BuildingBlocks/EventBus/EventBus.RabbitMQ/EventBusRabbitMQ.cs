using EventBus.Base.Configuration;
using EventBus.Base.Events;
using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.RabbitMQ
{
    public class EventBusRabbitMQ : BaseEventBus
    {
        RabbitMQPersistentConnection persistentConnection;
        private readonly IConnectionFactory connectionFactory;
        private readonly IModel consumerChannel;
        public EventBusRabbitMQ(EventBusConfig eventBusConfig, IServiceProvider serviceProvider) : base(eventBusConfig, serviceProvider)
        {
            if (eventBusConfig.Connection != null)
            {//casting object to ConnectionFactory.
                var connJson = JsonConvert.SerializeObject(eventBusConfig.Connection,
                    new JsonSerializerSettings()
                    {//Serilestirmede problem cikamamasi icin obje referanslarini yok sayiyorz. Iliskisel tablo objeleri ornek olarak gosterilebilir.
                        //Self referencing loop detected for property
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                connectionFactory = JsonConvert.DeserializeObject<ConnectionFactory>(connJson);
            }
            else
            {
                connectionFactory = new ConnectionFactory();
            }
            persistentConnection = new RabbitMQPersistentConnection(connectionFactory, eventBusConfig.ConnectionRetryCount);

            consumerChannel = CreateConsumerChannel();

            SubscrioptionManager.OnEventRemoved += SubscrioptionManager_OnEventRemoved;
        }

        private void SubscrioptionManager_OnEventRemoved(object? sender, string eventName)
        {
            eventName = ProcessEventName(eventName);
            if (!!persistentConnection.IsConnected)
            {
                persistentConnection.TryConnect();
            }

            consumerChannel.QueueUnbind(
                queue: eventName,
                exchange: EventBusConfig.DefaultTopicName,
                routingKey: eventName
                );
            if (SubscrioptionManager.IsEmpty)
            {
                consumerChannel.Close();
            }
        }

        public override void Subscribe<T, THandler>()
        {
            var eventName = typeof(T).Name;
            eventName = ProcessEventName(eventName);
            if (!SubscrioptionManager.HasSubscriptionForEvent(eventName))
            {
                if (!persistentConnection.IsConnected)
                {
                    persistentConnection.TryConnect();
                }

                consumerChannel.QueueDeclare(queue: GetSubName(eventName),//Ensurre queue exists while consuming
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                consumerChannel.QueueBind(queue: GetSubName(eventName),
                    exchange: EventBusConfig.DefaultTopicName,
                    routingKey: eventName);
            }

            SubscrioptionManager.AddSubscription<T, THandler>();
            StartBasicConsume(eventName);
        }

        public override void UnSubscribe<T, THandler>()
        {
            SubscrioptionManager.RemoveSubscription<T, THandler>();
        }

        public override void Publish(IntegrationEvent @event)
        {
            if (!persistentConnection.IsConnected)
            {
                persistentConnection.TryConnect();
            }

            var policy = Policy.Handle<SocketException>() //Burdaki hatalar alunduguda Execute calisacak
                 .Or<BrokerUnreachableException>()
                 .WaitAndRetry(EventBusConfig.ConnectionRetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                 {
                     //log
                  });

            var eventName = @event.GetType().Name;
            eventName = ProcessEventName(eventName);

            consumerChannel.ExchangeDeclare(exchange: EventBusConfig.DefaultTopicName, type: "direct");//Ensure exhange exists while publishing

            var message = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(message);

            policy.Execute(() =>
            {
                var properties = consumerChannel.CreateBasicProperties();
                properties.DeliveryMode = 2;//persistent

                consumerChannel.QueueDeclare(queue: GetSubName(eventName),
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                consumerChannel.BasicPublish(
                    exchange:EventBusConfig.DefaultTopicName,
                    routingKey:eventName,
                    mandatory:true,
                    basicProperties:properties,
                    body:body
                    );
            });
        }

        private IModel CreateConsumerChannel()
        {
            if (!persistentConnection.IsConnected)
            {
                persistentConnection.TryConnect();
            }
            var channel = persistentConnection.CreateModel();

            channel.ExchangeDeclare(exchange: EventBusConfig.DefaultTopicName, type: "direct");

            return channel;
        }

        private void StartBasicConsume(string eventName)
        {
            if (consumerChannel != null)
            {
                var consumer = new EventingBasicConsumer(consumerChannel);

                consumer.Received += Consumer_Received;

                consumerChannel.BasicConsume(
                    queue: GetSubName(eventName),
                    autoAck: false,
                    consumer: consumer
                    );
            }

        }

        private async void Consumer_Received(object? sender, BasicDeliverEventArgs e)
        {
            var eventName = e.RoutingKey;
            eventName = ProcessEventName(eventName);
            var message = Encoding.UTF8.GetString(e.Body.Span);

            try
            {
                await ProcessEvent(eventName, message);
            }
            catch (Exception ex)
            {
                //logg
                throw;
            }
            consumerChannel.BasicAck(e.DeliveryTag, multiple: false);
        }
    }
}
