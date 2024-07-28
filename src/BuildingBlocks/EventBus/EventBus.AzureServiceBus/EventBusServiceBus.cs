using EventBus.Base;
using EventBus.Base.Events;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;


namespace EventBus.AzureServiceBus
{
    public class EventBusServiceBus : BaseEventBus
    {
        private ITopicClient topicClient;
        private ManagementClient managementClient;
        private ILogger logger;
        public EventBusServiceBus(EventBusConfig eventBusConfig, IServiceProvider serviceProvider) : base(eventBusConfig, serviceProvider)
        {
            logger = serviceProvider.GetService(typeof(ILogger<EventBusServiceBus>)) as ILogger<EventBusServiceBus>;
            managementClient = new ManagementClient(eventBusConfig.EventBusConnectionString);
            topicClient = createTopicClient();
        }

        private ITopicClient createTopicClient()
        {
            if (topicClient == null || topicClient.IsClosedOrClosing)
            {
                topicClient = new TopicClient(EventBusConfig.EventBusConnectionString, EventBusConfig.DefaultTopicName, RetryPolicy.Default);
            }
            //Ensure that topic already exists 
            if (managementClient.TopicExistsAsync(EventBusConfig.DefaultTopicName).GetAwaiter().GetResult())
                managementClient.CreateTopicAsync(EventBusConfig.DefaultTopicName).GetAwaiter().GetResult();

            return topicClient;
        }

        public override void Publish(IntegrationEvent @event)
        {
            var eventName = @event.GetType().Name;// example: OrderCreatedIntegrationEvent
            eventName = ProcessEventName(eventName);// example: OrderCreated
            var eventStr = JsonConvert.SerializeObject(@event);
            var bodyArr = Encoding.UTF8.GetBytes(eventStr);

            var message = new Message()
            {
                MessageId = Guid.NewGuid().ToString(),
                Body = bodyArr,
                Label = eventName,
            };
            topicClient.SendAsync(message).GetAwaiter().GetResult(); //asnyc olmayan fonksiyonlarda await islemi yapanfonk.

        }
        public override void Subscribe<T, THandler>()
        {//HasSubscriptionForEvent bie eventName gondermeliyiz. Fakat Subscibe parametre almiyor T degerimiz integrationEvent.. temsil ettigig icin typeof ile aliriz.
            var eventName = typeof(T).Name;
            if (!SubscrioptionManager.HasSubscriptionForEvent(eventName))
            {

                createSubscriptionClient(eventName);
            }
        }
        public override void UnSubscribe<T, THandler>()
        {
            base.UnSubscribe<T, THandler>();
        }
        private ISubscriptionClient CreateSubscriptionClientIfNotExists(string eventName)
        {
            var subClient = createSubscriptionClient(eventName);
            var exists = managementClient.SubscriptionExistsAsync(EventBusConfig.DefaultTopicName, GetSubName(eventName))
                .GetAwaiter() // => asenkron bir metodu senkron olarak calistirmasini saglayan metod. Potansile Deadlock tehlikesi!
                .GetResult();

            if (!exists)
            {
                managementClient.CreateSubscriptionAsync(EventBusConfig.DefaultTopicName, GetSubName(eventName)).GetAwaiter().GetResult();
                RemoveDefaultRule(subClient);
                
            }
            CreateRuleIfNotExists(ProcessEventName(eventName), subClient);

            return subClient;
        }

        private void CreateRuleIfNotExists(string eventName, ISubscriptionClient subscriptionClient)
        {
            bool ruleExists;
            try
            {
                var rule = managementClient.GetRuleAsync(EventBusConfig.DefaultTopicName, eventName, eventName).GetAwaiter().GetResult();
                ruleExists = rule != null;

            }
            catch (MessagingEntityNotFoundException)
            {
                //Azure Management Client  doesn't have RuleExists method.
                ruleExists = false;
            }
            if (!ruleExists)
            {
                subscriptionClient.AddRuleAsync(new RuleDescription
                {
                    Filter = new CorrelationFilter { Label = eventName },
                    Name = eventName,
                }).GetAwaiter().GetResult();

            }
        }

        private void RemoveDefaultRule(SubscriptionClient subscriptionClient)
        {//Azure service icindeki default rule lari sildik.
            try
            {
                subscriptionClient
                    .RemoveRuleAsync(RuleDescription.DefaultRuleName)
                    .GetAwaiter().GetResult();
            }
            catch (MessagingEntityNotFoundException)
            {

                logger.LogWarning("The messaging entity {DefaultName} could not found.", RuleDescription.DefaultRuleName);
            }
        }

        private SubscriptionClient createSubscriptionClient(string eventName)
        {
            return new SubscriptionClient(EventBusConfig.EventBusConnectionString, EventBusConfig.DefaultTopicName, GetSubName(eventName));
        }

    }
}
