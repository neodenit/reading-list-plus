namespace ReadingListPlus.Services
{
    public interface ITextConverterService
    {
        string AddCloze(string initialText, string htmlSelection);

        string AddHighlight(string initialText, string htmlSelection);

        string DeleteTagByName(string initialText, string tagName);

        string DeleteTagByText(string initialText, string htmlSelection);

        string GetHtml(string text);

        string GetSelection(string text);

        string ReplaceTag(string text, string oldTag, string newTag);

        string AddParameter(string text, string tagName, string parameter);
    }
}