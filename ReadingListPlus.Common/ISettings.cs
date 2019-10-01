namespace ReadingListPlus.Common
{
    public interface ISettings
    {
        string SpacedRepetitionServer { get; }

        string ContactEmail { get; }

        bool AllowDeckSelection { get; }

        bool ShowHiddenCardsInIndex { get; }

        bool ResetKeysOnImport { get; }

        bool ExtractEnabled { get; }

        bool BookmarkEnabled { get; }

        bool RememberEnabled { get; }

        bool HighlightEnabled { get; }

        bool ClozeEnabled { get; }

        bool DropEnabled { get; }
    }
}
