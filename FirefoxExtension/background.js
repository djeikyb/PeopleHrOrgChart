/*
On startup, connect to the "ping_pong" app.
*/
const port = browser.runtime.connectNative("merviche.orgchart.nmhost");

console.log(port);

port.onDisconnect.addListener((p) => {
    console.log("Disconnected: " + p.name)
});

/*
Listen for messages from the app.
*/
port.onMessage.addListener((response) => {
    console.log("Received: " + response);
});

/*
On a click on the browser action, send the app a message.
*/
browser.browserAction.onClicked.addListener(() => {
    console.log("Sending:  ping");
    port.postMessage("ping");
});

function logURL(requestDetails) {
    console.log(`Loading: ${requestDetails.url}`);
    port.postMessage(requestDetails)
}

browser.webRequest.onBeforeRequest.addListener(logURL, {
    urls: ["https://*.peoplehr.accessacloud.com/*"],
});

browser.webRequest.onCompleted.addListener(logURL, {
    urls: ["https://*.peoplehr.accessacloud.com/*"],
});
