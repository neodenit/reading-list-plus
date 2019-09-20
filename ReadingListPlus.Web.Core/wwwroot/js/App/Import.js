$(function () {
    var fileElement = $('input[type="file"]');

    fileElement.change(function () {
        fileElement.closest('form').submit();
    });
});