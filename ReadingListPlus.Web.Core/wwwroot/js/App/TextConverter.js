var spanRegex = /<span class="(\w+)">([\s\S]+?)<\/span>/gi;

var brRegex = /<br\s*\/?>/gi;

function HtmlToText(html) {
	var noSpans = html.replace(spanRegex, '{{$1::$2}}');
	var noBR = noSpans.replace(brRegex, '\r\n');

	return noBR;
}
