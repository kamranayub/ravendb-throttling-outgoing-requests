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
                .AddSingleton<Client>()
                .AddSingleton<ExternalApi>()
                .BuildServiceProvider();

            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetService<Client>();

            Console.WriteLine($"Initiating client. Throttling to {Client.REQUEST_LIMIT} requests every {Client.SLIDING_TIME_WINDOW_IN_SECONDS} seconds. Press any key to exit.");

            do
            {
                while (!Console.KeyAvailable)
                {
                    await client.SendRequest();
                }
            } while (Console.ReadKey(true) == null);
        }
    }
}
