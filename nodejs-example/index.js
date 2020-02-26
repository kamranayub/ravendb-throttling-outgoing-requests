const readline = require("readline");
const client = require("./client");

// exit on any key
readline.emitKeypressEvents(process.stdin);
process.stdin.setRawMode(true);
process.stdin.on("keypress", process.exit);

console.log(
  `Initiating client. Throttling to ${client.REQUEST_LIMIT} requests every ${client.SLIDING_TIME_WINDOW_IN_SECONDS} seconds. Press any key to exit.`
);

const recurseSendClientRequest = async function () {
    await client.sendRequest();
    await recurseSendClientRequest();
};

recurseSendClientRequest();