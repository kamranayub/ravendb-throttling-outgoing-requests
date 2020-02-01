using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using dotnet_example.Configuration;
using Microsoft.Extensions.Options;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace dotnet_example
{


    public class SendRequests
    {
        IDocumentStore _store;
        ExternalApi _externalApi;

        public const int REQUEST_LIMIT = 30;
        public const int TTL_IN_SECONDS = 30;

        private class RateLimit
        {
            public string Id { get; set; }
        }

        public SendRequests(IOptions<RavenDB> ravendb, ExternalApi externalApi)
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
                var limiter = await session.LoadAsync<RateLimit>("rate_limit");
                IMetadataDictionary metadata;

                if (limiter == null)
                {
                    limiter = new RateLimit()
                    {
                        Id = "rate_limit"
                    };

                    await session.StoreAsync(limiter);

                    // expire rate limit every 30 seconds
                    metadata = session.Advanced.GetMetadataFor(limiter);
                    metadata.Add(
                        Raven.Client.Constants.Documents.Metadata.Expires,
                        DateTimeOffset.UtcNow.AddSeconds(TTL_IN_SECONDS)
                    );

                    await session.SaveChangesAsync();
                }
                else
                {
                    metadata = session.Advanced.GetMetadataFor(limiter);
                }

                DateTimeOffset expiresAt = DateTimeOffset.Parse(
                    metadata.GetString(Raven.Client.Constants.Documents.Metadata.Expires));

                var limitCounters = session.CountersFor(limiter.Id);
                var requests = await limitCounters.GetAsync("requests");

                if (requests != null && requests >= REQUEST_LIMIT)
                {
                    WriteRequestInfo(true, requests, expiresAt);
                    return;
                }

                WriteRequestInfo(false, requests, expiresAt);

                // track request
                limitCounters.Increment("requests");
                await session.SaveChangesAsync();

                // send external request
                await _externalApi.Fetch();
            }
        }

        private void WriteRequestInfo(bool reachedLimit, long? requests, DateTimeOffset expiresAt)
        {
            var remaining = DateTimeOffset.UtcNow - expiresAt;
            Console.Clear();
            Console.WriteLine($"Request Count: {requests ?? 0}/{REQUEST_LIMIT}, Throttled: {reachedLimit}, Expires In: {remaining.Duration()}");
        }
    }
}