var newLineRegex = /\r|\n/g;
var extractRegex = /<a href=".+?" class="(\w+)" data-id-param="([0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12})">([\s\S]+?)<\/a>/g;
var spanRegex = /<span class="(\w+)">([\s\S]+?)<\/span>/g;
var brRegex = /<br\s*\/?>/g;

function HtmlToText(html) {
    var textWithNoNewLines = html.replace(newLineRegex, '');
    var textWithNoExtracts = textWithNoNewLines.replace(extractRegex, '{{$1::$2::$3}}');
    var textWithNoSpans = textWithNoExtracts.replace(spanRegex, '{{$1::$2}}');
    var textWithNoBRs = textWithNoSpans.replace(brRegex, '\r\n');

    return textWithNoBRs;
}
