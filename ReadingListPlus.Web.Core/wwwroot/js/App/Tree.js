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
});