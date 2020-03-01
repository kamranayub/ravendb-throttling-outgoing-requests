## Node.js Example

- Link to article: https://www.codeproject.com/Articles/5260913/Throttling-Outgoing-HTTP-Requests-in-a-Distribut-2
- Video of demo running: https://youtu.be/YAstKXjtSEM

### Getting Started

1. Create a free [RavenDB Cloud](https://cloud.ravendb.com) instance
1. [Create a client certificate](https://ravendb.net/docs/article-page/4.2/csharp/server/security/authentication/certificate-management) for connecting via .NET
   1. Go into the Studio for your database in RavenDB Cloud
   1. On the left, go to Manage Server -> Certificates
   1. Create a new Client Certificate (blank passphrase OK for local dev)
   1. Extract to local folder and copy path to `.pfx` file
1. [Create a database](https://ravendb.net/docs/article-page/4.2/csharp/studio/server/databases/create-new-database/general-flow) within your RavenDB Cloud instance (e.g. `throttle`)
1. Install dependencies
    - `npm install`
1. Fill in `.env` file (see `.env.example` for an example)
   1. `RAVENDB_DATABASE_URLS` - your RavenDB instance URL
   1. `RAVENDB_DATABASE_NAME` - your RavenDB DB name (e.g. `throttle`)
   1. `RAVENDB_CERTIFICATE_PATH` - the full path to your Client Certificate (.pfx) file, downloaded from RavenDB Cloud. On Windows, be sure to use quotes and escape backslashes (e.g. `"C:\\bin\\cert.pfx"`)
1. Run `npm start`in as many terminal windows as you want

The app should start up and begin making "fake" outgoing API requests. After making 10 requests within 20 seconds, the app will display a message and will wait to send more requests until the time window has passed.

If you launch multiple processes, they will all respect the same expiration window and will not conflict with each other.

## Limitations of Demo

- Multiple processes could concurrently create a new rate limit document. To account for this, you could enable [optimistic concurrency](https://ravendb.net/docs/article-page/4.2/csharp/client-api/session/configuration/how-to-enable-optimistic-concurrency).
- Multiple processes could increment if request counter is `N - 1`, which would result in extra requests possibly causing an API exception (if your rate limit was exceeded)
- When the request limit is exceeded, the program retries in a tight loop. In a production app, you would be better off deferring execution until the time window has lapsed.
- You may notice that even when the document expires, it is not deleted. This is because by default documents that are expired [may take up to 60 seconds](https://ravendb.net/docs/article-page/4.2/csharp/server/extensions/expiration#eventual-consistency-considerations) to be removed.

These limitations could be worked around using more error checking but in the real world, these are unlikely to cause much of an issue with appropriate retry logic and API exception handling. For example, I use [message queueing](https://www.cloudamqp.com/blog/2014-12-03-what-is-message-queuing.html) for distributed scenarios like this.
