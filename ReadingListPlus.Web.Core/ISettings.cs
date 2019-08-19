namespace ReadingListPlus.Web.Core
{
    public interface ISettings
    {
        bool AllowDeckSelection { get; set; }

        bool ResetKeysOnImport { get; set; }
    }
}
