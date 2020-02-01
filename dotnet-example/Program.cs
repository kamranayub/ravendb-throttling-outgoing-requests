using System;
using System.Threading.Tasks;
using dotnet_example.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace dotnet_example
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            // tell the builder to look for the appsettings.json file
            builder
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder.AddUserSecrets<Program>();
            Configuration = builder.Build();

            IServiceCollection services = new ServiceCollection();
            services
                .Configure<RavenDB>(Configuration.GetSection(nameof(RavenDB)))
                .AddOptions()
                .AddSingleton<SendRequests>()
                .AddSingleton<ExternalApi>()
                .BuildServiceProvider();

            var serviceProvider = services.BuildServiceProvider();
            var sender = serviceProvider.GetService<SendRequests>();

            Console.WriteLine("Initiating sender. Throttling to 10 requests every 30 seconds. Press any key to exit.");

            do
            {
                while (!Console.KeyAvailable)
                {
                    await sender.SendRequest();
                }
            } while (Console.ReadKey(true) == null);
        }
    }
}
