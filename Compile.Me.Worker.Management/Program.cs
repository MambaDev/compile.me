using System;
using Compile.Me.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Compile.Me.Worker.Management
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureHostConfiguration(configHost =>
                {
                    configHost.AddEnvironmentVariables("DOTNET_");
                    configHost.AddJsonFile(
                        $"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")}.json", true, true);
                })
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true,
                        true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var config = hostContext.Configuration.GetSection("configuration").GetSection("compiler");

                    services.AddSingleton(new CompilerPublisher(config.GetValue<string>("publisher")));
                    services.AddHostedService<WorkerManagement>();
                })
                .UseSerilog();
    }
}