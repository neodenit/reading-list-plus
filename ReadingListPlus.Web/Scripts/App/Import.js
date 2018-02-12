$(function () {
    var fileElement = $('#File');

    fileElement.change(function () {
        fileElement.closest('form').submit();
    });
});