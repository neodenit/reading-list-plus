$(function () {
    function isMobile() {
        var minDesktopSize = 768;
        return screen.width < minDesktopSize;
    }

    function updateRowNumber() {
        var rows = isMobile() ? 3 : 10;
        $('textarea').prop('rows', rows);
    }

    function fixCursorPosition(element) {
        var len = element.value.length;
        element.setSelectionRange(len, len);
    }

    function fixPageCursorPosition() {
        var input = $('input[autofocus]')[0];

        if (input) {
            fixCursorPosition(input);
        } else {
            var textarea = $('textarea[autofocus]')[0];

            if (textarea && textarea.clientHeight >= textarea.scrollHeight) {
                fixCursorPosition(textarea);
            }
        }
    }

    function preventDoubleSubmit() {
        $('form').submit(function () {
            $(this).find('input[type="submit"]').prop('disabled', true);
        });
    }

    updateRowNumber();
    fixPageCursorPosition();
    preventDoubleSubmit();
});
