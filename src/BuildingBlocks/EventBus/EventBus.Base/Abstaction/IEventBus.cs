using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EventBus.Base.Events;

namespace EventBus.Base.Abstaction
{
    /// <summary>
    /// Burda servislerimizin subscription islemleri yapilcak.
    /// Hangi eventin subsrice edilecegini bununla belirliyoruz.
    /// </summary>
    public interface IEventBus : IDisposable
    {
        void Publish(IntegrationEvent @event);
        void Subscribe<T, THandler>() where T : IntegrationEvent where THandler : IIntegrationEventHandler<T>;
        void UnSubscribe<T, THandler>() where T : IntegrationEvent where THandler : IIntegrationEventHandler<T>;
    }
}
