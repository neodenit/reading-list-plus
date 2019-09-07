namespace ReadingListPlus.Web.Core
{
    public interface ISettings
    {
        string SpacedRepetionServer { get; }

        bool AllowDeckSelection { get; }

        bool ResetKeysOnImport { get; }

        bool ExtractEnabled { get; }

        bool BookmarkEnabled { get; }

        bool RememberEnabled { get; }

        bool HighlightEnabled { get; }

        bool ClozeEnabled { get; }

        bool DropEnabled { get; }
    }
}
