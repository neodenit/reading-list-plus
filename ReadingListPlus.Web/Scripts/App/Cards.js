$(function () {
    var actionsHeight = $('.actions').outerHeight(true);

    var maxClozeWidth = Math.max.apply(null, $('.cloze').map(function () {
        return $(this).width();
    }));

    $('.cloze').width(maxClozeWidth);

    $('#ScrollDown').click(function () {
        if ($('.extract').length) {
            DropSelections();

            var headerHeight = $('#scroll').offset().top;

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

    $('.highlight').click(function () {
        DropSelections();
        $(this).removeClass('highlight').addClass('highlightselected');
    });

    $('.extract').click(function () {
        DropSelections();
        $(this).removeClass('extract').addClass('extractselected');
    });

    $('.cloze').click(function () {
        DropSelections();
        $(this).removeClass('cloze').addClass('clozeselected');
    });

    $('.act').click(function () {
        if (this.id == 'Extract') {
            var htmlText0 = $('#article').html();

            var isValid = checkSelection();

            if (isValid) {
                var newNode = createSelectionSpan();

                surroundSelection(newNode);

                var htmlText1 = $('#article').html();

                clean();

                var htmlText2 = $('#article').html();

                var selection = getSelectionSpan();

                var isSelectionExists = checkForExistence(selection);

                if (isSelectionExists) {
                    var htmlText = $('#article').html();
                    var convertedText = HtmlToText(htmlText);
                    var trimmedText = convertedText.trim();

                    SubmitSelection(trimmedText, this.id)
                }
            }
        } else if (this.id == 'DeleteRegion') {
            if ($('.highlightselected').length) {
                var pattern = getPatternFromTag('.highlightselected');

                SubmitSelection(pattern, this.id);
            } else if ($('.extractselected').length) {
                var pattern = getPatternFromTag('.extractselected');

                SubmitSelection(pattern, this.id);
            } else if ($('.clozeselected').length) {
                var pattern = getPatternFromTag('.clozeselected');

                SubmitSelection(pattern, this.id);
            }
        } else {
            var text = getSelectionText();

            var trimmed = $.trim(text);

            if (trimmed.length > 0) {
                var escaped = GetWords(trimmed);

                SubmitSelection(escaped, this.id);
            }
        }
    });

    $(window).scroll(function () {
        var headerHeight = $('#scroll').offset().top;

        if ($(window).scrollTop() >= headerHeight) {
            $('.actions').css('position', 'fixed');
            $('.actions').css('top', 0);
        }
        else {
            $('.actions').css('position', 'relative');
        }
    });
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
    } else if (document.selection && document.selection.type != 'Control') {
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

function getSelectionText2() {
    if (window.getSelection) {
        var sel1 = window.getSelection();
        var range1 = sel1.getRangeAt(0);
        var content1 = range1.toString();

        var text1 = sel1.toString();

        var sel2 = getSelection();
        var range2 = sel2.getRangeAt(0);
        var content2 = range2.toString();

        var text2 = sel2.toString();

        var r1 = this.Range.valueOf();
        var r2 = this.Range.toString();

        return content;
    } else if (document.selection && document.selection.type != 'Control') {
        return document.selection.createRange().text;
    } else {
        return '';
    }
}
function getSelectionText3() {
    var newNode = document.createElement('span');

    newNode.className = 'highlight';

    surroundSelection(newNode);

    return selectionContents;
}

function checkSelection() {
    var node = window.getSelection().focusNode.parentNode;
    return node.id == 'article';
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
    var result = element.length ? true : false;

    return result;
}

function switchClass(element, from, to) {
    element.removeClass(from).addClass(to);
}