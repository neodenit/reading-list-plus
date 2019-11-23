using System;
using System.Collections.Generic;

namespace ReadingListPlus.Services
{
    public interface ITextConverterService
    {
        string AddCloze(string initialText, string htmlSelection);

        string AddHighlight(string initialText, string htmlSelection);

        string DeleteTagByName(string initialText, string tagName);

        string DeleteTagByNameAndParam(string initialText, string tagName, Guid param);

        string DeleteTagByText(string initialText, string htmlSelection);

        string GetHtml(string text, string cardUrlTemplate, string repetitionCardUrlTemplate, string newRepetitionCardUrlTemplate, string newRepetitionCardClass);

        string GetTagText(string text, string tagName);

        string ReplaceTag(string text, string oldTag, string newTag);

        string AddParameter(string text, string tagName, string parameter);

        string GetIdParameter(string text, string tagName);

        string GetNewRepetitionCardText(string text);

        string GetPatternForSelection(string text);

        string GetPatternForDeletion(string text);

        IEnumerable<string> GetTagNames(string text);

        IEnumerable<string> GetTags(string text, string tagName);
    }
}