## .NET Core Example

1. Create a free test [RavenDB Cloud](https://cloud.ravendb.com) database
1. Download your admin certificate (warning: only use this for local development)
1. This example uses `dotnet secrets` to manage your connection to a RavenDB Cloud instance.
    1. Run `dotnet user-secrets init`
    1. Run `dotnet user-secrets set ravendb.url <RAVENDB_URL>` replacing the token with your RavenDB instance URL
    1. Run `dotnet user-secrets set ravendb.db <RAVENDB_DATABASE_NAME>` replacing the token with your RavenDB DB name
    1. Run `dotnet user-secrets set ravendb.certPath <CERT_PATH>` replacing the token with the full path to your Admin Certificate Public Key X509 certificate (.crt) file
1. Now run `dotnet run`

The app should start up, seed the database, and begin making "fake" outgoing API requests. After making 10 requests within 20 seconds, the app will display a message and will wait to send more requests until the time window has passed.

If you launch multiple processes, they will all respect the same expiration window and will not conflict with each other.