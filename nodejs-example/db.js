const fs = require("fs");
const config = require("dotenv-safe").config();
const { DocumentStore } = require("ravendb");

const authOptions = {
  certificate: fs.readFileSync(config.parsed.RAVENDB_CERTIFICATE_PATH),
  type: "pfx"
};
const store = new DocumentStore(
  config.parsed.RAVENDB_DATABASE_URLS.split(";"),
  config.parsed.RAVENDB_DATABASE_NAME,
  authOptions
);

store.initialize();

module.exports = {
  openSession() {
    return store.openSession();
  }
};
