var baseUrl = "https://knowledge-manager.azurewebsites.net";

function getCurrentTab(callback) {
    var queryInfo = {
        active: true,
        currentWindow: true
    };

    chrome.tabs.query(queryInfo, function (tabs) {
        var tab = tabs[0];

        callback(tab);
    });
}

function isNewTab(tab) {
    return tab.url === "chrome://newtab/";
}

document.addEventListener('DOMContentLoaded', function () {
    var addArticle = document.getElementById("addArticle");

    addArticle.addEventListener("click", function () {
        getCurrentTab(function (tab) {
            var newURL = baseUrl + "/Cards/CreateFromUrl?url=" + encodeURIComponent(tab.url);
            chrome.tabs.create({ url: newURL });
        });
    });

    var addExtract = document.getElementById("addExtract");

    addExtract.addEventListener("click", function () {
        chrome.tabs.executeScript({
            code: "window.getSelection().toString();"
        }, function (selection) {
            var text = selection[0];

            if (text) {
                var newURL = baseUrl + "/Cards/Create?text=" + encodeURIComponent(text);
                chrome.tabs.create({ url: newURL });
            } else {
                alert("Please select the text first.");
            }
        });
    });

    var openList = document.getElementById("openList");

    openList.addEventListener("click", function () {
        var newURL = baseUrl;

        getCurrentTab(function (tab) {
            if (isNewTab(tab)) {
                chrome.tabs.update(tab.id, { url: newURL });
                window.close();
            } else {
                chrome.tabs.create({ url: newURL });
            }
        });
    });
});
