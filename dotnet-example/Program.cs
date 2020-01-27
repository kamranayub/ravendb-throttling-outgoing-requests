using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Raven.Client.Documents;

namespace throttling_ravendb
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
                
            var store = new DocumentStore() {
                Urls = new string[] {

                },
                Database = ""
            };

            store.Initialize();
            
            using (var session = store.OpenAsyncSession()) 
            {

            }
        }
    }
}
