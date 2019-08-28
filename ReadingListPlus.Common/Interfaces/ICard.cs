using System;

namespace ReadingListPlus.Common.Interfaces
{
    public interface ICard
    {
        Guid ID { get; set; }

        int Position { get; set; }
    }
}
