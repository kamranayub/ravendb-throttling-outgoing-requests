using System;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace throttling_ravendb {

  public class ExternalApi {
    private IAsyncDocumentSession session;

    private class RateLimit {
      public string Id { get; set; }
    }

    public ExternalApi(IAsyncDocumentSession session)
    {
        this.session = session;
    }
    public async Task Fetch() {
      var limiter = await session.LoadAsync<RateLimit>("rate_limit");

      if (limiter == null) {
        limiter = new RateLimit() {
          Id = "rate_limit"
        };

        await session.StoreAsync(limiter);
        await session.SaveChangesAsync();
      }

      var limitCounters = session.CountersFor(limiter.Id);
      var requests = await limitCounters.GetAsync("requests");

      if (requests.HasValue && requests.Value >= 50) {
        throw new ApiRateLimitException("Reached rate limit of 50 requests");
      }

      limitCounters.Increment("requests");

      await session.SaveChangesAsync();

      // simulate external latency
      await Task.Delay(500);
    }
  }

  [Serializable]
  internal class ApiRateLimitException : Exception
  {
    public ApiRateLimitException()
    {
    }

    public ApiRateLimitException(string message) : base(message)
    {
    }

    public ApiRateLimitException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected ApiRateLimitException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
  }
}