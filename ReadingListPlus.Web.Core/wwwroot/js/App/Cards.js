$(function () {
    var mainPanelHeight = $('#mainPanel').outerHeight(true);
    var navbarHeight = $('nav.fixed-top').outerHeight(true);

    var maxClozeWidth = Math.max.apply(null, $('.cloze').map(function () {
        return $(this).width();
    }));

    $('.cloze').width(maxClozeWidth);

    document.onselectionchange = function () {
        var selection = window.getSelection();

        var anchorNodeClassName =
            selection.anchorNode &&
            selection.anchorNode.className;

        var focusNodeClassName =
            selection.focusNode &&
            selection.focusNode.className;

        var anchorNodeParentClassName =
            selection.anchorNode &&
            selection.anchorNode.parentNode &&
            selection.anchorNode.parentNode.className;

        var focusNodeParentClassName =
            selection.focusNode &&
            selection.focusNode.parentNode &&
            selection.focusNode.parentNode.className;

        var isValid =
            (anchorNodeClassName === 'article' || anchorNodeParentClassName === 'article') &&
            (focusNodeClassName === 'article' || focusNodeParentClassName === 'article') &&
            selection.toString();

        if (isValid) {
            $('.selection-panel a[data-act].btn-primary').removeClass('disabled');
        } else {
            $('.selection-panel a[data-act].btn-primary').addClass('disabled');
        }
    };

    $('.bookmark').click(function () {
        DropSelections();
        $(this).removeClass('bookmark').addClass('bookmarkselected');
    });

    $('.highlight').click(function () {
        DropSelections();
        $(this).removeClass('highlight').addClass('highlightselected');
    });

    $('span.extract').click(function () {
        DropSelections();
        $(this).removeClass('extract').addClass('extractselected');
    });

    $('.cloze').click(function () {
        DropSelections();
        $(this).removeClass('cloze').addClass('clozeselected');
    });

    $('a[data-act]').click(function () {
        var action = $(this).data('act');

        if (action === 'Extract' || action === 'Bookmark' || action === 'Remember') {
            var newNode = createSelectionSpan();

            surroundSelection(newNode);

            clean();

            var selection = getSelectionSpan();

            var isSelectionExists = checkForExistence(selection);

            if (isSelectionExists) {
                var htmlText = $('#article').html();
                var convertedText = HtmlToText(htmlText);
                var trimmedText = convertedText.trim();

                SubmitSelection(trimmedText, action);
            }
        } else if (action === 'DeleteRegion') {
            var regionTypes = ['.bookmarkselected', '.highlightselected', '.extractselected', '.clozeselected'];

            for (var i in regionTypes) {
                var regionType = regionTypes[i];

                if ($(regionType).length) {
                    var pattern = getPatternFromTag(regionType);
                    SubmitSelection(pattern, action);
                    break;
                }
            }
        } else if (action === 'CancelRepetitionCardCreation' || action === 'CompleteRepetitionCardCreation') {
            SubmitSelection("", action);
        } else {
            var text = getSelectionText();

            var trimmed = $.trim(text);

            if (trimmed.length > 0) {
                var escaped = GetWords(trimmed);

                SubmitSelection(escaped, action);
            }
        }
    });

    var postpone = function (priority) {
        $('#Card_Priority').val(priority);

        var isBookmarked = $('#Card_IsBookmarked').val();
        var cardType = $('#Card_Type').val();

        if (cardType !== 'Article' || isBookmarked === 'True') {
            SubmitSelection("", "Postpone");
        } else if (isBookmarked === 'False') {
            $('#ModalDialog').modal();

            $('#YesButton').unbind('click').click(function () {
                SubmitSelection("", "Postpone");
            });
        }
    };

    $('#PostponeHigh').click(function () {
        postpone('High');
    });

    $('#PostponeMedium').click(function () {
        postpone('Medium');
    });

    $('#PostponeLow').click(function () {
        postpone('Low');
    });

    $('#mainPanel').sticky({ topSpacing: navbarHeight });

    if ($('.bookmark').length) {
        var position = $('.bookmark').offset().top - navbarHeight - mainPanelHeight;

        $('html, body').animate({
            scrollTop: position
        }, 'slow');
    }
});

function ReplaceBR(text) {
    return text.replace(/<\s*br\s*\/?\s*>/ig, '\r\n');
}

function GetWords(text) {
    return text.replace(/\W+/g, '\\W+');
}

function htmlDecode(value) {
    return value.replace('&lt;', '<').replace('&gt;', '>').replace('&amp;', '&');
}

function DropSelections() {
    $('.bookmarkselected').removeClass('bookmarkselected').addClass('bookmark');
    $('.highlightselected').removeClass('highlightselected').addClass('highlight');
    $('.extractselected').removeClass('extractselected').addClass('extract');
    $('.clozeselected').removeClass('clozeselected').addClass('cloze');
}

function SubmitSelection(selection, action) {
    $('#Card_Selection').val(selection);
    $('#Card_NextAction').val(action);

    $('#myForm').submit();
}

function getSelectionText() {
    if (window.getSelection) {
        return window.getSelection().toString();
    } else if (document.selection && document.selection.type !== 'Control') {
        return document.selection.createRange().text;
    } else {
        return '';
    }
}

function getPatternFromTag(selector) {
    var selected = $(selector);
    var html = selected.html();
    var text = htmlDecode(html);
    var noBR = ReplaceBR(text);
    var escaped = GetWords(noBR);

    return escaped;
}

function checkSelection() {
    var node = window.getSelection().focusNode.parentNode;
    return node.id === 'article';
}

function createSelectionSpan() {
    var newNode = document.createElement('span');

    newNode.className = 'selection';

    return newNode;
}

function surroundSelection(element) {
    try {
        if (window.getSelection) {
            var sel = window.getSelection();
            if (sel.rangeCount) {
                var range = sel.getRangeAt(0).cloneRange();
                range.surroundContents(element);
                sel.removeAllRanges();
                sel.addRange(range);
            }
        }
    } catch (e) {
        console.log(e.message);
    }
}

function clean() {
    $('span > span').each(function () {
        $(this).replaceWith(this.childNodes);
    });
}

function getSelectionSpan() {
    var result = $('.selection');

    return result;
}

function checkForExistence(element) {
    var result = !!element.length;
    return result;
}

function switchClass(element, from, to) {
    element.removeClass(from).addClass(to);
}
