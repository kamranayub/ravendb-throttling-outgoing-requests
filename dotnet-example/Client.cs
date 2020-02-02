using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using dotnet_example.Configuration;
using Microsoft.Extensions.Options;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Raven.Client.Exceptions;
using Raven.Client.Exceptions.Documents;

namespace dotnet_example
{
    public class Client
    {
        IDocumentStore _store;
        ExternalApi _externalApi;

        public const string RATE_LIMIT_ID = "RateLimit/ExternalApi";
        public const int REQUEST_LIMIT = 30;
        public const int SLIDING_TIME_WINDOW_IN_SECONDS = 30;

        private class RateLimit
        {
            public string Id { get; set; }
        }

        public Client(IOptions<RavenDB> ravendb, ExternalApi externalApi)
        {
            var config = ravendb.Value ?? throw new ArgumentNullException(nameof(ravendb));

            Console.WriteLine($"[RavenDB.DatabaseUrls] = {config.DatabaseUrls[0]}");
            Console.WriteLine($"[RavenDB.DatabaseName] = {config.DatabaseName}");
            Console.WriteLine($"[RavenDB.Certificate] = {config.CertPath}");

            _store = new DocumentStore()
            {
                Urls = config.DatabaseUrls,
                Database = config.DatabaseName,
                Certificate = new X509Certificate2(config.CertPath)
            };

            _store.Initialize();
            _externalApi = externalApi;
        }

        public async Task SendRequest()
        {
            using (var session = _store.OpenAsyncSession())
            {
                var limiter = await session.LoadAsync<RateLimit>(RATE_LIMIT_ID);

                if (limiter == null)
                {
                    limiter = new RateLimit()
                    {
                        Id = RATE_LIMIT_ID
                    };

                    await session.StoreAsync(limiter);

                    // expire rate limit every 30 seconds
                    var metadata = session.Advanced.GetMetadataFor(limiter);
                    metadata.Add(
                        Raven.Client.Constants.Documents.Metadata.Expires,
                        DateTimeOffset.UtcNow.AddSeconds(SLIDING_TIME_WINDOW_IN_SECONDS)
                    );

                    await session.SaveChangesAsync();
                }

                // Get available counters
                var limitCounters = session.CountersFor(limiter);

                // Check existing request limit
                var existingRequests = await limitCounters.GetAsync("requests");

                if (existingRequests != null && existingRequests >= REQUEST_LIMIT)
                {
                    WriteRequestInfo(true, existingRequests);

                    // Abort request. In a more sophisticated solution we could defer,
                    // exponentially back-off, or re-enqueue the message at a later time.
                    return;
                }

                WriteRequestInfo(false, existingRequests);

                try
                {
                    // increment request counter to prepare
                    limitCounters.Increment("requests");
                    await session.SaveChangesAsync();
                }
                catch (DocumentDoesNotExistException)
                {
                    // OK, the document just expired  
                    // We'll try again since we don't want to
                    // generate another request  
                    return;
                }
            }

            // send external request
            await _externalApi.Fetch();
        }

        private void WriteRequestInfo(bool reachedLimit, long? requests)
        {
            Console.Clear();
            Console.WriteLine($"Request Count: {requests ?? 0}/{REQUEST_LIMIT}, Throttled: {reachedLimit}");
        }
    }
}