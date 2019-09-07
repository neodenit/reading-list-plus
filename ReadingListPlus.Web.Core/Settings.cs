namespace ReadingListPlus.Web.Core
{
    public class Settings : ISettings
    {
        public string SpacedRepetionServer { get; set; }

        public bool AllowDeckSelection { get; set; }

        public bool ResetKeysOnImport { get; set; }

        public bool ExtractEnabled { get; set; }

        public bool BookmarkEnabled { get; set; }

        public bool RememberEnabled { get; set; }

        public bool HighlightEnabled { get; set; }

        public bool ClozeEnabled { get; set; }

        public bool DropEnabled { get; set; }
    }
}
