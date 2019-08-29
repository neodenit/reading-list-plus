$(function () {
    var actionsHeight = $('#mainPanel').outerHeight(true);
    var topBar = $('#top-bar');
    var topBarHeight = topBar.height();
    var headerHeight = $('#scrollArea').offset().top;

    var maxClozeWidth = Math.max.apply(null, $('.cloze').map(function () {
        return $(this).width();
    }));

    $('.cloze').width(maxClozeWidth);

    $('#ScrollDown').click(function () {
        if ($('.extract').length) {
            DropSelections();

            $('.extract:last').before('<span class="top" />');

            var position =
                $(window).scrollTop() < headerHeight ?
                $('.top').offset().top - actionsHeight * 2 :
                $('.top').offset().top - actionsHeight;

            $('html, body').animate({
                scrollTop: position
            }, 'slow', 'swing');

            $('.top').remove();
        }
    });

    $('#ScrollUp').click(function () {
        $('html, body').animate({
            scrollTop: 0
        }, 'slow', 'swing');
    });

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

    $('.act').click(function () {
        var action = this.id;

        if (action === 'Extract' || action === 'Bookmark') {
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
        $('#Priority').val(priority);

        var isBookmarked = $('#IsBookmarked').val();
        var cardType = parseInt($('#Type').val());

        if (cardType !== Enums.CardType.Article || isBookmarked === 'True') {
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

    var throttledScroll = $.throttle(100, function () {
        if ($(window).scrollTop() >= headerHeight) {
            $('#mainPanel').css('position', 'fixed');
            $('#mainPanel').css('top', topBarHeight);
        } else {
            $('#mainPanel').css('position', 'relative');
            $('#mainPanel').css('top', 0);
        };
    });

    $(window).scroll(throttledScroll);

    if ($('.bookmark').length) {
        $('#mainPanel').css('position', 'fixed');
        $('#mainPanel').css('top', topBarHeight);

        var position = $('.bookmark').offset().top - topBarHeight - actionsHeight;

        $('#mainPanel').css('position', 'relative');

        $('html, body').animate({
            scrollTop: position
        }, 'slow', 'swing');
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
    $('#Selection').val(selection);
    $('#NextAction').val(action);

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
