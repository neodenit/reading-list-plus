namespace ReadingListPlus
{
    public interface IDeck
    {
    }

    public interface ICard
    {
        int Position { get; set; }

        bool IsNew { get; set; }
    }
}
