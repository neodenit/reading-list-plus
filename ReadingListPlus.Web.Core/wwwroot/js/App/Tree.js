'use strict';

$(function () {
    $('#tree').jstree({
        'core': {
            'data': {
                'url': function (node) {
                    return node.id === '#'
                        ? '/Decks/Tree'
                        : '/Cards/Tree';
                },
                'data': function (node) {
                    return { 'id': node.id };
                }
            }
        }
    });

    $('#tree').on('click', 'a[href]', function () {
        location = $(this).prop('href');
    });
});