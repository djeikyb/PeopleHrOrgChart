/*
On startup, connect to the "ping_pong" app.
*/
let current_port = browser.runtime.connectNative("merviche.orgchart.nmhost");
connectup(current_port)

function connectup(port) {

    console.log(port);

    port.onDisconnect.addListener((p) => {
        console.log("Disconnected: " + p.name)
        current_port = browser.runtime.connectNative("merviche.orgchart.nmhost");
        connectup(current_port)
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

    function logURL(details) {
        console.log(`Will pass this along: ${details.url}`);

        console.log(details)

        let filter = browser.webRequest.filterResponseData(details.requestId);

        let decoder = new TextDecoder("utf-8");
        let encoder = new TextEncoder();
        let str = '';
        filter.ondata = (event) => {
            console.log("🥑 onDATA")
            str += decoder.decode(event.data, {stream: true});
            filter.write(encoder.encode(str));
        };

        filter.onstart = (event) => {
            console.log("🥑 onSTART")
            str = '';
        };

        filter.onerror = (event) => {
            console.log("🥑 onERROR")
        };

        filter.onstop = (event) => {
            console.log("🥑 onSTOP")
            console.log(str)
            port.postMessage(JSON.parse(str));
            filter.disconnect();
        };

        return {};
    }

    browser.webRequest.onBeforeRequest.addListener(logURL, {
        urls: ["https://*.peoplehr.accessacloud.com/Interfaces/OrganisationChart.aspx"],
        types: ["main_frame", "xmlhttprequest"],
    }, ["blocking"]);
}
