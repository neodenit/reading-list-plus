namespace ReadingListPlus.Web.Core
{
    public class Settings : ISettings
    {
        public bool AllowDeckSelection { get; set; }

        public bool ResetKeysOnImport { get; set; }

        public bool RememberEnabled { get; set; }

        public string SpacedRepetionServer { get; set; }
    }
}
