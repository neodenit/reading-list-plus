using ReadingListPlus.Common.Enums;

namespace ReadingListPlus.Common
{
    public class Settings : ISettings
    {
        public string SpacedRepetitionServer { get; set; }

        public string ContactEmail { get; set; }

        public bool AllowDeckSelection { get; set; }

        public bool AllowHighestPriority { get; set; }

        public bool ShowHiddenCardsInIndex { get; set; }

        public bool ResetKeysOnImport { get; set; }

        public bool FixOnImport { get; set; }

        public bool ExtractEnabled { get; set; }

        public bool BookmarkEnabled { get; set; }

        public bool RememberEnabled { get; set; }

        public bool HighlightEnabled { get; set; }

        public bool ClozeEnabled { get; set; }

        public bool DropEnabled { get; set; }

        public bool EnableArticlePrioritySelection { get; set; }

        public bool EnableExtractPrioritySelection { get; set; }

        public bool EnableCommonNotePrioritySelection { get; set; }

        public Priority DefaultArticlePriority { get; set; }

        public Priority DefaultExtractPriority { get; set; }

        public Priority DefaultCommonNotePriority { get; set; }
    }
}
