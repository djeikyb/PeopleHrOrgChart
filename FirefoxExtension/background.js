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

    // Tap requests for the json org chart. This is a tenant-specific subdomain
    // (the wildcard) at the peoplehr host, an http post to:
    //
    //     POST /Interfaces/OrganisationChart.aspx
    //
    // There are a bunch of profile pictures that could be cached as well for
    // display in the desktop app.
    browser.webRequest.onBeforeRequest.addListener(wiretap, {
        urls: ["https://*.peoplehr.accessacloud.com/Interfaces/OrganisationChart.aspx"],
        types: ["main_frame", "xmlhttprequest"],
    }, ["blocking"]);

    /**
     * <p>
     * The body is copied from the response stream and broadcast to exactly two
     * recipients: (a) the original destination (b) a native messaging host.
     * </p>
     * <p>
     * This could be rather expensive, so take care to only tap specific http
     * interactions.
     * </p>
     * <p>
     * https://www.enterpriseintegrationpatterns.com/patterns/messaging/WireTap.html
     * </p>
     */
    function wiretap(details) {
        console.log(`Will pass this along: ${details.url}`);

        // TODO bail if not POST method?
        //      could also check rq/rs content type?
        //      or is that covered by the xmlhttprequest part of the browser.webRequest.RequestFilter?

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
}
