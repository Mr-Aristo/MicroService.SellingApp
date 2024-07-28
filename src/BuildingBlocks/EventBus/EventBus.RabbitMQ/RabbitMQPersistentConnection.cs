using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.RabbitMQ
{
    public class RabbitMQPersistentConnection : IDisposable
    {//Rabbitte connectionun baglanip ve baglantinin devamli olmasi gerekmekte
     //azurede boyle bir sikintimiz yokdu
     //Rabbitmq da connection nesnesi olusturabilmek icin bir connection factorye ihtyiyac var.

        private readonly IConnectionFactory connectionFactory;
        private readonly int retryCount;
        private IConnection connection;
        private object lock_object = new object();
        private bool _dispose; //dispose edeilip edilmediginin mesajini tutacagiz.
        public RabbitMQPersistentConnection(IConnectionFactory connectionFactory, int retryCount = 5)
        {
            this.connectionFactory = connectionFactory;
            this.retryCount = retryCount;
        }
        public bool IsConnected => connection != null && connection.IsOpen;

        public IModel CreateModel()
        {
            return connection.CreateModel();
        }
        public void Dispose()
        {
            _dispose = true;
            connection.Dispose();
        }

        public bool TryConnect()
        {//TryConnect birden fazla kez cagirabilir. Tekrar cagirildiginda lock sayesinde bir onceki islemin bitmesini bekliyecek.

            lock (lock_object)
            {
                var policy = Policy.Handle<SocketException>() //Burdaki hatalar alunduguda Execute calisacak
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                    {

                    });

                policy.Execute(() =>
                {
                    connection = connectionFactory.CreateConnection();
                   
                });

                if (IsConnected)
                {
                    connection.ConnectionShutdown += Connection_ConnectionShutdown;
                    connection.CallbackException += Connection_CallbackException;
                    connection.ConnectionBlocked += Connection_ConnectionBlocked;
                    //log

                    return true;
                }
                return false;

            }

        }

        private void Connection_ConnectionBlocked(object? sender, ConnectionBlockedEventArgs e)
        {
            if (_dispose) return; //dispose edildigi icin kapandiysa hata baglanma dedik. 
            TryConnect();
        }

        private void Connection_CallbackException(object? sender, CallbackExceptionEventArgs e)
        {
            if (_dispose) return;
            TryConnect();
        }

        private void Connection_ConnectionShutdown(object? sender, ShutdownEventArgs e)
        {
            if (_dispose) return;
            //log

            TryConnect();
        }
    }
}
