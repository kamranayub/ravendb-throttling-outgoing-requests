## .NET Core Example

1. Create a free [RavenDB Cloud](https://cloud.ravendb.com) instance
1. [Create a client certificate](https://ravendb.net/docs/article-page/4.2/csharp/server/security/authentication/certificate-management) for connecting via .NET
   1. Go into the Studio for your database in RavenDB Cloud
   1. On the left, go to Manage Server -> Certificates
   1. Create a new Client Certificate (blank passphrase OK for local dev)
   1. Extract to local folder and copy path to `.pfx` file
1. [Create a database](https://ravendb.net/docs/article-page/4.2/csharp/studio/server/databases/create-new-database/general-flow) within your RavenDB Cloud instance (e.g. `throttle`)
1. This example uses `dotnet secrets` to manage your connection to a RavenDB Cloud instance.
   1. Run `dotnet user-secrets init`
   1. Run `dotnet user-secrets set RavenDB:DatabaseUrls:0 "<RAVENDB_URL>"` replacing the token with your RavenDB instance URL
   1. Run `dotnet user-secrets set RavenDB.DatabaseName "<RAVENDB_DATABASE_NAME>"` replacing the token with your RavenDB DB name
   1. Run `dotnet user-secrets set RavenDB.CertPath "<CERT_PATH>"` replacing the token with the full path to your Client Certificate (.pfx) file, downloaded from RavenDB Cloud
1. Now run `dotnet run`

The app should start up, seed the database, and begin making "fake" outgoing API requests. After making 10 requests within 20 seconds, the app will display a message and will wait to send more requests until the time window has passed.

If you launch multiple processes, they will all respect the same expiration window and will not conflict with each other.
