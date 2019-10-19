using System;

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

        string GetSelection(string text);

        string ReplaceTag(string text, string oldTag, string newTag);

        string AddParameter(string text, string tagName, string parameter);

        string GetIdParameter(string text, string tagName);

        string GetNewRepetitionCardText(string text);

        string GetTextPattern(string text);
    }
}