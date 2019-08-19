namespace ReadingListPlus.Web.Core
{
    public class Settings : ISettings
    {
        public bool AllowDeckSelection { get; set; }

        public bool ResetKeysOnImport { get; set; }
    }
}
