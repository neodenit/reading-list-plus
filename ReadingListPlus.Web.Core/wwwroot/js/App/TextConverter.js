var extractRegex = /<a href=".+?" class="extract" data-card-id="([0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12})">([\s\S]+?)<\/a>/g;
var spanRegex = /<span class="(\w+)">([\s\S]+?)<\/span>/g;
var brRegex = /<br\/>/g;

function HtmlToText(html) {
    var textWithNoExtracts = html.replace(extractRegex, '{{extract::$1::$2}}');
    var textWithNoSpans = textWithNoExtracts.replace(spanRegex, '{{$1::$2}}');
    var textWithNoBRs = textWithNoSpans.replace(brRegex, '\r\n');

    return textWithNoBRs;
}
