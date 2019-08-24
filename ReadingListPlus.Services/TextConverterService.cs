using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ReadingListPlus.Services
{
    public class TextConverterService : ITextConverterService
    {
        public string AddHighlight(string initialText, string htmlSelection)
        {
            return GetReplacement(initialText, htmlSelection, "highlight");
        }

        public string AddCloze(string initialText, string htmlSelection)
        {
            return GetReplacement(initialText, htmlSelection, "cloze");
        }

        public string GetHtml(string text)
        {
            var html = TextToHtml(text);

            return html;
        }

        public string GetSelection(string text)
        {
            var result = Regex.Match(text, @"{{selection::(?s)(.+?)}}").Groups[1].Value;

            return result;
        }

        public string ReplaceTag(string text, string oldTag, string newTag) =>
            Regex.Replace(text, $"{{{{{oldTag}::(?s)(.+?)}}}}", $"{{{{{newTag}::$1}}}}");

        public string DeleteTagByText(string initialText, string htmlSelection)
        {
            var pattern = @"{{\w+::(" + htmlSelection + ")}}";

            var result = Regex.Replace(initialText, pattern, "$1", RegexOptions.IgnoreCase);

            return result;
        }

        public string DeleteTagByName(string initialText, string tagName) =>
            Regex.Replace(initialText, $"{{{{{tagName}::(?s)(.+?)}}}}", "$1", RegexOptions.IgnoreCase);

        private string GetReplacement(string initialText, string htmlSelection, string tag)
        {
            var isValid = Validate(htmlSelection);

            if (!isValid)
            {
                return initialText;
            }
            else
            {
                var matches = from Match match in Regex.Matches(initialText, @"{{\w+::.+?}}", RegexOptions.Singleline) select match;

                var trimmedSelection = Regex.Replace(htmlSelection, @"\\W\+(.+)\\W\+", "$1");

                var pattern = string.Format(@"\b{0}\b", htmlSelection);

                var finalText = Regex.Replace(
                    initialText,
                    pattern,
                    match => MatchEvaluator(match, matches, tag),
                    RegexOptions.IgnoreCase);

                return finalText;
            }
        }

        private bool Validate(string text)
        {
            var letters = Regex.IsMatch(text, @"\w");

            var isValid = letters;

            return isValid;
        }

        private string TextToHtml(string text)
        {
            var htmlText1 = Regex.Replace(text, Environment.NewLine, "<br />");

            var htmlText2 = Regex.Replace(htmlText1, @"{{(\w+)::(.+?)}}", @"<span class=""$1"">$2</span>");

            return htmlText2;
        }

        private string MatchEvaluator(Match match, IEnumerable<Match> matches, string tag)
        {
            var start1 = match.Index;
            var end1 = match.Index + match.Length;

            foreach (var item in matches)
            {
                var start2 = item.Index;
                var end2 = item.Index + item.Length;

                if (Math.Max(start1, start2) < Math.Min(end1, end2))
                {
                    return match.Value;
                }
            }

            var matchWithoutTags = Regex.Replace(match.Value, @"{{\w+::(.+?)}}", "$1");

            var result = GetTag(matchWithoutTags, tag);

            return result;
        }

        private string GetTag(string text, string tag)
        {
            return "{{" + tag + "::" + text + "}}";
        }
    }
}
