'use strict';

$(function () {
    $('#import-button').click(function () {
        var url = $('#Card_Url').val();
        var escapedUrl = encodeURIComponent(url);
        var deckId = $('#Card_DeckID').val();
        location = '/Cards/CreateFromUrl?url=' + escapedUrl + '&deckId=' + deckId;
    });
});