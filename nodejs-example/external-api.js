const fetch = function () {

    // fake HTTP request that simulates 500ms of network latency
    const promise = new Promise((resolve) => {
        setTimeout(resolve, 500);
    })

    return promise;
}

module.exports = {
    fetch
};