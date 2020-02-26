## .NET Core Example

- Link to article: https://www.codeproject.com/Articles/5260137/Throttling-Outgoing-HTTP-Requests-in-a-Distributed
- Video of demo running: https://youtu.be/YAstKXjtSEM

### Getting Started

1. Create a free [RavenDB Cloud](https://cloud.ravendb.com) instance
1. [Create a client certificate](https://ravendb.net/docs/article-page/4.2/csharp/server/security/authentication/certificate-management) for connecting via .NET
   1. Go into the Studio for your database in RavenDB Cloud
   1. On the left, go to Manage Server -> Certificates
   1. Create a new Client Certificate (blank passphrase OK for local dev)
   1. Extract to local folder and copy path to `.pfx` file
1. [Create a database](https://ravendb.net/docs/article-page/4.2/csharp/studio/server/databases/create-new-database/general-flow) within your RavenDB Cloud instance (e.g. `throttle`)
1. Create user secrets
   1. Run `dotnet user-secrets init`
   1. Run `dotnet user-secrets set RavenDB:DatabaseUrls:0 "<RAVENDB_URL>"` replacing the token with your RavenDB instance URL
   1. Run `dotnet user-secrets set RavenDB.DatabaseName "<RAVENDB_DATABASE_NAME>"` replacing the token with your RavenDB DB name (e.g. `throttle`)
   1. Run `dotnet user-secrets set RavenDB.CertPath "<CERT_PATH>"` replacing the token with the full path to your Client Certificate (.pfx) file, downloaded from RavenDB Cloud
1. Run `dotnet publish`
1. Navigate to `bin\Debug\netcoreapp3.1` and run the `throttling-ravendb.exe` program. Open the program multiple times to demonstrate separate isolated processes being throttled.

The app should start up and begin making "fake" outgoing API requests. After making 10 requests within 20 seconds, the app will display a message and will wait to send more requests until the time window has passed.

If you launch multiple processes, they will all respect the same expiration window and will not conflict with each other.

## Limitations of Demo

- Multiple processes could concurrently create a new rate limit document. To account for this, you could enable [optimistic concurrency](https://ravendb.net/docs/article-page/4.2/csharp/client-api/session/configuration/how-to-enable-optimistic-concurrency).
- Multiple processes could increment if request counter is `N - 1`, which would result in extra requests possibly causing an API exception (if your rate limit was exceeded)
- When the request limit is exceeded, the program retries in a tight loop. In a production app, you would be better off deferring execution until the time window has lapsed.
- You may notice that even when the document expires, it is not deleted. This is because by default documents that are expired [may take up to 60 seconds](https://ravendb.net/docs/article-page/4.2/csharp/server/extensions/expiration#eventual-consistency-considerations) to be removed.

These limitations could be worked around using more error checking but in the real world, these are unlikely to cause much of an issue with appropriate retry logic and API exception handling. For example, I use [message queueing](https://www.cloudamqp.com/blog/2014-12-03-what-is-message-queuing.html) and [Polly](https://github.com/App-vNext/Polly) for distributed scenarios like this.
