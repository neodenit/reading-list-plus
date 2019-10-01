namespace ReadingListPlus.Common
{
    public class Constants
    {
        public const int DisconnectedCardPosition = -1;
        public const int FirstCardPosition = 0;

        public const string BackupPolicy = "BackupPolicy";
        public const string BackupClaim = "BackupClaim";
        public const string FixPolicy = "FixPolicy";
        public const string FixClaim = "FixClaim";

        public const string GuidRegex = "[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}";

        public const string TagGroup = "Tag";
        public const string IdGroup = "Id";
        public const string TextGroup = "Text";

        public const string NewRepetitionCardLabel = "newRepetition";
        public const string RepetitionCardLabel = "repetition";
        public const string SelectionLabel = "selection";
        public const string ExtractLabel = "extract";

        public const string ViewTitle = "Title";
        public const string ViewMessage = "Message";

        public const string Ellipsis = "…";
        public const int MaxIndexTextLength = 256;
        public const int MaxTreeTextLength = 128;
        public const int MaxDeleteTextLength = 128;
    }
}
