using System.Security.Authentication.ExtendedProtection;
using EventBus.Base.Abstaction;
using EventBus.Base.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;//ServiceCollection

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
        public void subscribe_event_on_rabbitmq_test()
        {
           // services.AddSingleton<IEventBus, BaseEventBus>();//DI her IEventBus istendiginde, sunu yap

            //Testi gerceklestirebilmek icin serivceProvidere ihtiyacimiz var. Bu sekilde olusturmus olduk.
            var sp = services.BuildServiceProvider();

        }
    }
}