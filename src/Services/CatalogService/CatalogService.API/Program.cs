using CatalogService.API.Extentions;
using CatalogService.API.Infrastructure.Context;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using CatalogService.API.Infrastructure;

namespace CatalogService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("Configurations/appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"Configurations/appsettings.{env}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var serilogConfiguration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("Configurations/serilog.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"Configurations/serilog.{env}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(serilogConfiguration)
                .CreateLogger();

            try
            {
                Log.Information("Starting web host");

                var builder = WebApplication.CreateBuilder(args);

                // Configure services
                builder.Services.ConfigureDbContext(builder.Configuration);
                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                builder.Services.Configure<CatalogSettings>(configuration.GetSection("CatalogSettings"));

                builder.Host.ConfigureAppConfiguration(i => i.AddConfiguration(configuration));
                builder.WebHost.UseWebRoot("Pics");
                builder.WebHost.UseContentRoot(Directory.GetCurrentDirectory());
                builder.Host.ConfigureLogging(i => i.ClearProviders().AddSerilog());

                var app = builder.Build();

                 app.MigrateDbContext<CatalogContext>(async (context, services) =>
                {
                    var env = services.GetRequiredService<IWebHostEnvironment>();
                    var logger = services.GetRequiredService<ILogger<CatalogContextSeed>>();
                    await new CatalogContextSeed().SeedAsync(context, env, logger);
                });

                // Configure the HTTP request pipeline
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();
                app.UseAuthentication(); // If authentication is used
                app.UseAuthorization();
                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
