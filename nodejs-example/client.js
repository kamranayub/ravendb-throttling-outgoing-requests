const { addSeconds } = require("date-fns");
const { DocumentDoesNotExistException } = require("ravendb");
const db = require("./db");
const externalApi = require("./external-api");

const RATE_LIMIT_ID = "RateLimit/ExternalApi";
const REQUEST_LIMIT = 30;
const SLIDING_TIME_WINDOW_IN_SECONDS = 30;

function logRequestInfo(reachedLimit, requestCount) {
  console.clear();
  console.log(
    `Request Count: ${requestCount ||
      0}/${REQUEST_LIMIT}, Throttled: ${reachedLimit}`
  );
}

module.exports = {
  RATE_LIMIT_ID,
  REQUEST_LIMIT,
  sendRequest: async function() {
    const session = db.openSession();

    let limiter = await session.load(RATE_LIMIT_ID, {
      includes(includes) {
        return includes.includeCounter("requests");
      }
    });

    if (limiter === null) {
      limiter = {
        id: RATE_LIMIT_ID
      };

      await session.store(limiter);

      // expire rate limit every 30 seconds
      const metadata = session.advanced.getMetadataFor(limiter);
      metadata["@expires"] = addSeconds(
        new Date(),
        SLIDING_TIME_WINDOW_IN_SECONDS
      );

      await session.saveChanges();
    }

    // Get available counters
    const limitCounters = session.countersFor(limiter);

    // Check existing request limit
    const existingRequests = await limitCounters.get("requests");

    if (existingRequests !== null && existingRequests >= REQUEST_LIMIT) {
      logRequestInfo(true, existingRequests);

      // Abort request. In a more sophisticated solution we could defer,
      // exponentially back-off, or re-enqueue the message at a later time.
      return;
    }

    logRequestInfo(false, existingRequests);

    try {
      // increment request counter to prepare
      limitCounters.increment("requests");
      await session.saveChanges();
    } catch (error) {
      if (error instanceof DocumentDoesNotExistException) {
        // OK, the document just expired
        // We'll try again since we don't want to
        // generate another request
        return;
      }

      throw error;
    }

    // send external request
    await externalApi.fetch();
  }
};
