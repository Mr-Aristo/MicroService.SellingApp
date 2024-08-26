using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Base.Configuration
{
    public class EventBusConfig
    {
        public int ConnectionRetryCount { get; set; } = 5;
        public string DefaultTopicName { get; set; } = "SellingAppEventBus";
        public string EventBusConnectionString { get; set; } = string.Empty;
        public string SubscriberClientAppName { get; set; } = string.Empty; //Hangi service bir queue yaratacak
        public string EventNamePrefix { get; set; } = string.Empty;
        public string EventNameSuffix { get; set; } = "IntegrationEvent";
        public EventBusType EventBusType { get; set; } = EventBusType.RabbitMQ;

        public object Connection { get; set; }//Bu prop object olamasinin sebebi disardan bir servis bu base i kullanirken
        //Bu EventBus.Base kullanan servisler object yerine koyulan belirli bir objenin dllnide kullanmak zorunda kalacaklardi
        //genel bir yapi icin object olarak tanimladik.

        public bool DeleteEventPrefix => !string.IsNullOrEmpty(EventNamePrefix);
        public bool DeleteEventSuffix => !string.IsNullOrEmpty(EventNameSuffix);
    }
    public enum EventBusType
    {
        RabbitMQ = 0,
        AzurServiceBus = 1
    }
}
