﻿'use strict';

$(function () {
    var mainPanelHeight = $('#mainPanel').outerHeight(true);
    var navbarHeight = $('nav.fixed-top').outerHeight(true);

    var maxClozeWidth = Math.max.apply(null, $('.cloze').map(function () {
        return $(this).width();
    }));

    $('.cloze').width(maxClozeWidth);

    document.onselectionchange = checkSelection;

    var selectionClassMapping = {
        "bookmark": "bookmarkselected",
        "highlight": "highlightselected",
        "cloze": "clozeselected"
    };

    for (var oldClass in selectionClassMapping) {
        var newClass = selectionClassMapping[oldClass];
        bindSelectionEvent(oldClass, newClass);
    }

    $('#article').on('click', 'span.bookmarkselected, span.highlightselected, span.clozeselected, span.extractselected', function () {
        dropSelection();
        $('#delete-panel').addClass('d-none');
    });

    $('#article').on('click', 'span.extract', function () {
        dropSelection();
        switchClass($(this), 'extract', 'selection');

        var articleText = getArticleText();

        submitSelection(articleText, 'Extract');
    });

    $('a[data-act]').click(function () {
        var button = $(this);
        var action = button.data('act');

        switch (action) {
            case 'Extract':
            case 'Bookmark':
            case 'Remember':
                dropSelection();

                var newNode = createSelectionSpan();
                surroundSelection(newNode);

                removeChildSpans();

                var articleText = getArticleText();

                submitSelection(articleText, action);

                break;
            case 'DeleteRegion':
                var regionTypes = ['.bookmarkselected', '.highlightselected', '.extractselected', '.clozeselected'];

                for (var i in regionTypes) {
                    var regionType = regionTypes[i];

                    if ($(regionType).length) {
                        var regionText = $(regionType).text();
                        submitSelection(regionText, action);
                        break;
                    }
                }

                break;
            case 'CancelRepetitionCardCreation':
            case 'CompleteRepetitionCardCreation':
                submitSelection('', action);
                break;
            case 'Highlight':
            case 'Cloze':
                var selectionText = getSelectionText();
                submitSelection(selectionText, action);
                break;
            case 'Postpone':
                var priority = button.data('priority');
                $('#Card_Priority').val(priority);

                checkPosition(action);

                break;
        }
    });

    $('#mainPanel').sticky({ topSpacing: navbarHeight });

    scrollToLastSelection();

    function checkPosition(action) {
        var position = parseInt($('#Card_Position').val());

        if (position !== 0) {
            var positionMessage = $('#PositionMessage').val();

            showModal(positionMessage, function () {
                checkBookmark(action);
            });
        } else {
            checkBookmark(action);
        }
    }

    function checkBookmark(action) {
        var isBookmarked = $('#IsBookmarked').val();
        var cardType = $('#Card_CardType').val();

        if (cardType !== 'Article' || isBookmarked === 'True') {
            submitSelection('', action);
        } else if (isBookmarked === 'False') {
            var bookmarkMessage = $('#BookmarkMessage').val();

            showModal(bookmarkMessage, function () {
                submitSelection('', action);
            });
        }

    }

    function showModal(message, yesButtonCallback) {
        $('#comfirmation-message').text(message);

        $('#ModalDialog').modal('show');

        $('#YesButton').off('click').on('click', function () {
            if (yesButtonCallback) {
                $('#ModalDialog').one('hidden.bs.modal', function () {
                    yesButtonCallback();
                });
            }

            $('#ModalDialog').modal('hide');
        });
    }

    function scrollToLastSelection() {

        var targetExtractId = $('#Extract').val();

        if (targetExtractId) {
            var targetExtract = $('[data-id-param="' + targetExtractId + '"]');
            var targetExtractOffset = targetExtract.offset().top;

            scrollTo(targetExtractOffset);
        } else {
            var bookmark = $('.bookmark');

            if (bookmark.length) {
                var bookmarkOffset = bookmark.offset().top;

                scrollTo(bookmarkOffset);
            } else {
                var lastExtract = $('.lastExtract');

                if (lastExtract.length) {
                    var lastExtractOffset = lastExtract.offset().top;

                    scrollTo(lastExtractOffset);
                } else {
                    var extractOffset = Math.max.apply(null, $('.extract').map(function () {
                        return $(this).offset().top;
                    }));

                    if (extractOffset) {
                        scrollTo(extractOffset);
                    }
                }
            }
        }
    }

    function scrollTo(offset) {
        var position = offset - navbarHeight - mainPanelHeight;

        $('html, body').animate({
            scrollTop: position
        }, 'slow');

    }

    function bindSelectionEvent(unselectedClass, selectedClass) {
        var selector = 'span' + '.' + unselectedClass;

        $('#article').on('click', selector, function () {
            dropSelection();
            switchClass($(this), unselectedClass, selectedClass);
            $('#delete-panel').removeClass('d-none');
        });
    }

    function dropSelection() {
        $('.bookmarkselected').removeClass('bookmarkselected').addClass('bookmark');
        $('.highlightselected').removeClass('highlightselected').addClass('highlight');
        $('.clozeselected').removeClass('clozeselected').addClass('cloze');
    }

    function submitSelection(selection, action) {
        $('#Card_Selection').val(selection);
        $('#Card_NextAction').val(action);

        $('#myForm').submit();
    }

    function getSelectionText() {
        return window.getSelection().toString();
    }

    function getArticleText() {
        var htmlText = $('#article').html();
        var convertedText = HtmlToText(htmlText);
        var trimmedText = convertedText.trim();
        return trimmedText;
    }

    function checkSelection() {
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
            /\w/.test(selection.toString());

        if (isValid) {
            $('.selection-panel a[data-act].btn-primary').removeClass('disabled');
        } else {
            $('.selection-panel a[data-act].btn-primary').addClass('disabled');
        }
    }

    function createSelectionSpan() {
        var newNode = document.createElement('span');
        newNode.className = 'selection';
        return newNode;
    }

    function surroundSelection(element) {
        var sel = window.getSelection();

        if (sel.rangeCount) {
            var range = sel.getRangeAt(0).cloneRange();
            range.surroundContents(element);
            sel.removeAllRanges();
            sel.addRange(range);
        }
    }

    function removeChildSpans() {
        $('span > span').each(function () {
            $(this).replaceWith(this.childNodes);
        });
    }

    function switchClass(element, from, to) {
        element.removeClass(from).addClass(to);
    }
});
