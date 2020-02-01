using System;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace dotnet_example
{

    public class ExternalApi
    {
        public ExternalApi()
        {
        }
        public async Task Fetch()
        {
            // simulate external latency
            await Task.Delay(500);
        }
    }
}