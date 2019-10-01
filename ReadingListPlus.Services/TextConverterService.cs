using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using ReadingListPlus.Common;

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

        public string GetHtml(string text, string cardUrlTemplate, string repetitionCardUrlTemplate, string newRepetitionCardUrlTemplate, string newRepetitionCardClass)
        {
            var encodedText = WebUtility.HtmlEncode(text);
            var html = ConvertTemplateToHtml(encodedText, cardUrlTemplate, repetitionCardUrlTemplate, newRepetitionCardUrlTemplate, newRepetitionCardClass);
            return html;
        }

        public string GetSelection(string text)
        {
            var result = Regex.Match(text, $@"{{{{{Constants.SelectionLabel}::(?s)(.+?)(?m)}}}}").Groups[1].Value;

            return result;
        }

        public string ReplaceTag(string text, string oldTag, string newTag) =>
            Regex.Replace(text, $"{{{{{oldTag}::({Constants.GuidRegex}::)?(?s)(.+?)(?m)}}}}", $"{{{{{newTag}::$1$2}}}}");

        public string DeleteTagByText(string initialText, string htmlSelection)
        {
            var pattern = @"{{\w+::(" + htmlSelection + ")}}";

            var result = Regex.Replace(initialText, pattern, "$1", RegexOptions.IgnoreCase);

            return result;
        }

        public string DeleteTagByName(string initialText, string tagName) =>
            Regex.Replace(initialText,
                $"{{{{{tagName}::(?<{Constants.IdGroup}>{Constants.GuidRegex}::)?(?s)(?<{Constants.TextGroup}>.+?)(?m)}}}}",
                $"${{{Constants.TextGroup}}}");

        public string DeleteTagByNameAndParam(string initialText, string tagName, Guid param) =>
            Regex.Replace(initialText,
                $"{{{{{tagName}::(?<{Constants.IdGroup}>{param}::)(?s)(?<{Constants.TextGroup}>.+?)(?m)}}}}",
                $"${{{Constants.TextGroup}}}");

        private string GetReplacement(string initialText, string htmlSelection, string tag)
        {
            var isValid = Validate(htmlSelection);

            if (!isValid)
            {
                return initialText;
            }
            else
            {
                var matches = from Match match in Regex.Matches(initialText, @"{{\w+::.+?}}") select match;

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

        public string AddParameter(string text, string tagName, string parameter) =>
            Regex.Replace(text, $"{{{{{tagName}::(?s)(.+?)(?m)}}}}", $"{{{{{tagName}::{parameter}::$1}}}}");

        public string GetIdParameter(string text, string tagName) =>
            Regex.Match(text, $"{{{{{tagName}::(?<{Constants.IdGroup}>{Constants.GuidRegex})::(?s).+?(?m)}}}}").Groups[Constants.IdGroup].Value;

        public string GetNewRepetitionCardText(string text) =>
             Regex.Match(text,
                $"{{{{{Constants.NewRepetitionCardLabel}::(?<{Constants.IdGroup}>{Constants.GuidRegex})::(?s)(?<{Constants.TextGroup}>.+?)(?m)}}}}").Groups[Constants.TextGroup].Value;

        private bool Validate(string text)
        {
            var letters = Regex.IsMatch(text, @"\w");

            var isValid = letters;

            return isValid;
        }

        private string ConvertTemplateToHtml(string template, string cardUrlTemplate, string repetitionCardUrlTemplate, string newRepetitionCardUrlTemplate, string newRepetitionCardClass)
        {
            var htmlText1 = Regex.Replace(template, Environment.NewLine, "<br/>");
            var htmlText2 = Regex.Replace(htmlText1,
                $"{{{{{Constants.ExtractLabel}::(?<{Constants.IdGroup}>{Constants.GuidRegex})::(?s)(?<{Constants.TextGroup}>.+?)(?m)}}}}",
                $@"<a href='{cardUrlTemplate}' class=""{Constants.ExtractLabel}"" data-id-param=""${{{Constants.IdGroup}}}"">${{{Constants.TextGroup}}}</a>");

            var htmlText3 = Regex.Replace(htmlText2,
                $"{{{{{Constants.NewRepetitionCardLabel}::(?<{Constants.IdGroup}>{Constants.GuidRegex})::(?s)(?<{Constants.TextGroup}>.+?)(?m)}}}}",
                $@"<a href='{newRepetitionCardUrlTemplate}' class=""{newRepetitionCardClass}"" data-id-param=""${{{Constants.IdGroup}}}"">${{{Constants.TextGroup}}}</a>");

            var htmlText4 = Regex.Replace(htmlText3,
                $"{{{{{Constants.RepetitionCardLabel}::(?<{Constants.IdGroup}>{Constants.GuidRegex})::(?s)(?<{Constants.TextGroup}>.+?)(?m)}}}}",
                $@"<a href='{repetitionCardUrlTemplate}' class=""{Constants.RepetitionCardLabel}"" data-id-param=""${{{Constants.IdGroup}}}"">${{{Constants.TextGroup}}}</a>");

            var htmlText5 = Regex.Replace(htmlText4, @"{{(\w+)::(?s)(.+?)(?m)}}", @"<span class=""$1"">$2</span>");

            return htmlText5;
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
