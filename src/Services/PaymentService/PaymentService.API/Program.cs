using EventBus.Base;
using EventBus.Base.Abstaction;
using EventBus.Base.Configuration;
using EventBus.Factory;
using Microsoft.OpenApi.Models;
using PaymentService.Api.IntegrationEvents.EventHandlers;
using PaymentService.API.IntegrationEvents.Events;
using RabbitMQ.Client;
using Serilog;

namespace PaymentService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //serilog.json dosyasini kullanabilmek icin ortam basli degisken olusturmamiz gereklidir.
            /*Uygulamanın hangi ortamda(Development, Staging, Production) çalıştığını belirlemek için 
             * ASPNETCORE_ENVIRONMENT ortam değişkenini alır.
             * Bu, ortam bazlı farklı konfigürasyon dosyalarının yüklenmesini sağlar.*/
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            /*Uygulamanın genel yapılandırmasını (appsettings.json dosyası) yükler.
              Ortama özel ayarları (örn. appsettings.Development.json veya appsettings.Production.json) yükler. 
            Bu ayarlar, env değişkenine bağlı olarak dinamik olarak yüklenir.*/
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"Configurations/appsettings.json", optional: false)
                .AddJsonFile($"Configurations/appsettings.{env}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var serilogConfiguration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"Configurations/serilog.json", optional: false)
                .AddJsonFile($"Configurations/serilog.{env}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(serilogConfiguration)
                .CreateLogger();

            Log.Logger.Information("Application is Starting...");

            var builder = WebApplication.CreateBuilder(args);

            // Serilog'u kullanmak için yapılandırma
            builder.Logging.ClearProviders();
            builder.Host.UseSerilog();

            // Servisleri ekleme
            builder.Services.AddTransient<OrderStartedIntegrationEventHandler>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PaymentService.Api", Version = "v1" });
            });

            // EventBus Configuration
            builder.Services.AddSingleton<IEventBus>(sp =>
            {
                EventBusConfig config = new()
                {
                    ConnectionRetryCount = 5,
                    EventNameSuffix = "IntegrationEvent",
                    SubscriberClientAppName = "PaymentService",
                    EventBusType = EventBusType.RabbitMQ,
                    Connection = new ConnectionFactory()
                    {
                        HostName = "localhost",
                        Port = 15672,
                        UserName = "guest",
                        Password = "guest"
                    },
                };

                return EventBusFactory.Create(config, sp);
            });
            
            var app = builder.Build();

            // HTTP request pipeline yapılandırması
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PaymentService.Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            // EventBus subscribe işlemi
            IEventBus eventBus = app.Services.GetRequiredService<IEventBus>();
            eventBus.Subscribe<OrderStartedIntegrationEvent, OrderStartedIntegrationEventHandler>();

            app.Run();
        }
    }
}
