using ReadingListPlus.Common.Enums;

namespace ReadingListPlus.Common
{
    public interface ISettings
    {
        string SpacedRepetitionServer { get; }

        string ContactEmail { get; }

        bool AllowDeckSelection { get; }

        bool AllowHighestPriority { get; }

        bool ShowHiddenCardsInIndex { get; }

        bool ResetKeysOnImport { get; }

        bool FixOnImport { get; }

        bool ExtractEnabled { get; }

        bool BookmarkEnabled { get; }

        bool RememberEnabled { get; }

        bool HighlightEnabled { get; }

        bool ClozeEnabled { get; }

        bool DropEnabled { get; }

        bool EnableArticlePrioritySelection { get; }

        bool EnableExtractPrioritySelection { get; }

        bool EnableCommonNotePrioritySelection { get; }

        Priority DefaultArticlePriority { get; }

        Priority DefaultExtractPriority { get; }

        Priority DefaultCommonNotePriority { get; }
    }
}
