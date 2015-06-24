var spanRegex = /<span .*?class="(\w+)".*?>(.+?)<\/span>/gi;

var spanStartRegex = /<span .*?class="\w+".*?>/gi;

var spanEndRegex = /<\/span>/gi;

var brRegex = /<br\s*\/?>/gi;

function addCloze(text, selection) {
	var words = getWords(selection);

	text.replace(words, clozeReplacer);
}

function addHighlight(text, selection) {
	var words = getWords(selection);

	text.replace(words, highlightReplacer);
}

function getWords(text) {
	var trimmedSelection = text.replace(/^\W*(.+?)\W*$/, '$1');

	var words = trimmedText.replace(/\W+/g, '\\W+');

	return words;
}

function clozeReplacer(match, offset, string) {
	return replacer(match, offset, string, 'cloze');
}

function highlightReplacer(match, offset, string) {
	return replacer(match, offset, string, 'highlight');
}

function replacer(match, offset, string, tag) {
	var start1 = offset;
	var end1 = offset + match.input.Length;


	var matches = getMatches(string, spanRegex);

	for (var i in matches) {
		var start2 = matches[i].index;
		var end2 = matches[i].index + matches[i].input.length;

		if (Math.Max(start1, start2) < Math.Min(end1, end2)) {
			return match.input;
		}
	}

	var openTag = '<span class="' + tag + '">';
	var closeTag = '</span>';

	var result = openTag + match.input + closeTag;

	return result;
}

function getMatches(text, re) {
	while (m = re.exec(text)) {
		return m;
	}
}

function addExtract(text, start, end) {
	var startPart = text.slice(0, start);
	var selection = text.slice(start, end);
	var endPart = text.slice(end, text.length);

	var start1 = start;
	var end1 = end;

	var cleanSelection = selection.replace(spanRegex, '$2');

	var openTag = '<span class="extract">';
	var closeTag = '</span>';

	var matches = getMatches(text, spanRegex);

	for (var i in matches) {
		var start2 = matches[i].index;
		var end2 = matches[i].index + matches[i].input.length;

		if (Math.Max(start1, start2) < Math.Min(end1, end2)) {
			return text;
		}
	}

	if (!cleanSelection.match(spanStartRegex) && !cleanSelection.match(spanEndRegex)) {
		var result = startPart + openTag + cleanSelection + closeTag + endPart;

		return result;
	}
}

function HtmlToText(html) {
	var noSpans = html.replace(spanRegex, '{{$1::$2}}');
	var noBR = noSpans.replace(brRegex, '\r\n');

	return noBR;
}
