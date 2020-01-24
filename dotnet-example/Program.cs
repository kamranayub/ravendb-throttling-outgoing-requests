using System;
using System.Threading.Tasks;
using Raven.Client.Documents;

namespace throttling_ravendb
{
    class Program
    {
        static async Task Main(string[] args)
        {
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
