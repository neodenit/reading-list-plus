$(function () {
    function isMobile() {
        var minDesktopSize = 768;
        return screen.width < minDesktopSize;
    }

    var rows = isMobile() ? 3 : 10;
    $('textarea').prop('rows', rows);

    $('form').submit(function () {
        $(this).find('input[type="submit"]').prop('disabled', true);
    });
});