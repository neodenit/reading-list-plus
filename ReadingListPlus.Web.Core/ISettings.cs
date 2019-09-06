namespace ReadingListPlus.Web.Core
{
    public interface ISettings
    {
        bool AllowDeckSelection { get; set; }

        bool ResetKeysOnImport { get; set; }

        bool RememberEnabled { get; set; }

        string SpacedRepetionServer { get; set; }
    }
}
