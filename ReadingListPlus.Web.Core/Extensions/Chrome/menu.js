var baseUrl = "https://knowledge-manager-demo.azurewebsites.net";

var menuItem = chrome.contextMenus.create(
    {
        "title": "Import Article",
        "contexts": ["link"],
        "onclick": function (info) {
            if (info.linkUrl) {
                var newURL = baseUrl + "/Cards/CreateFromUrl?url=" + encodeURIComponent(info.linkUrl);
                chrome.tabs.create({ url: newURL });
            }
        }
    });
