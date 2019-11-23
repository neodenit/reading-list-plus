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
        public string AddHighlight(string initialText, string selectionPattern)
        {
            return GetReplacement(initialText, selectionPattern, Constants.HighlightLabel);
        }

        public string AddCloze(string initialText, string selectionPattern)
        {
            return GetReplacement(initialText, selectionPattern, Constants.ClozeLabel);
        }

        public string GetHtml(string text, string cardUrlTemplate, string repetitionCardUrlTemplate, string newRepetitionCardUrlTemplate, string newRepetitionCardClass)
        {
            var encodedText = WebUtility.HtmlEncode(text);
            var html = ConvertTemplateToHtml(encodedText, cardUrlTemplate, repetitionCardUrlTemplate, newRepetitionCardUrlTemplate, newRepetitionCardClass);
            return html;
        }

        public string GetTagText(string text, string tagName)
        {
            var result = Regex.Match(text, $@"{{{{{tagName}::({Constants.GuidRegex}::)?(?s)(.+?)(?m)}}}}").Groups[2].Value;

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

        private string GetReplacement(string initialText, string selectionPattern, string tag)
        {
            var isValid = ValidateSelectionPattern(selectionPattern);

            if (!isValid)
            {
                return initialText;
            }
            else
            {
                var matches = from Match match in Regex.Matches(initialText, @"{{\w+::.+?}}") select match;

                var finalText = Regex.Replace(
                    initialText,
                    selectionPattern,
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

        public string GetPatternForSelection(string text)
        {
            var trimmedString = TrimSpecialCharacters(text);
            var wordPattern = GetWordPattern(trimmedString);
            var pattern = $@"\b{wordPattern}\b";
            return pattern;
        }

        public string GetPatternForDeletion(string text)
        {
            var trimmedString = TrimSpecialCharacters(text);
            var wordPattern = GetWordPattern(trimmedString);
            var pattern = $@"\W*{wordPattern}\W*";
            return pattern;
        }

        public IEnumerable<string> GetTagNames(string text) =>
            Regex.Matches(text, @"{{(\w+)::(?s)(.+?)(?m)}}")
            .Cast<Match>()
            .Select(m => m.Groups[1].Value);
                

        private string TrimSpecialCharacters(string text) =>
            Regex.Replace(text, @"^\W*(.+?)\W*$", "$1");

        private string GetWordPattern(string text) =>
            Regex.Replace(text, @"\W+", @"\W+");


        private bool ValidateSelectionPattern(string text)
        {
            var isValid = Regex.IsMatch(text, @"\w");
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

        private string MatchEvaluator(Match newTagCandidate, IEnumerable<Match> existingTags, string tag)
        {
            var newTagStart = newTagCandidate.Index;
            var newTagEnd = newTagCandidate.Index + newTagCandidate.Length;

            foreach (var existingTag in existingTags)
            {
                var existingTagStart = existingTag.Index;
                var existingTagEnd = existingTag.Index + existingTag.Length;

                if (Overlap(newTagStart, newTagEnd, existingTagStart, existingTagEnd))
                {
                    return newTagCandidate.Value;
                }
            }

            string result = GenerateTag(newTagCandidate.Value, tag);
            return result;
        }

        private static bool Overlap(int start1, int end1, int start2, int end2) =>
            Math.Max(start1, start2) < Math.Min(end1, end2);

        private string GenerateTag(string text, string tag) =>
            $"{{{{{tag}::{text}}}}}";

        public IEnumerable<string> GetTags(string text, string tagName) =>
            Regex.Matches(text, @$"{{{{{tagName}::(?s)(.+?)(?m)}}}}")
            .Cast<Match>()
            .Select(m => m.Value);
    }
}
