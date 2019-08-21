using System.Collections.Generic;
using ReadingListPlus.Common;
using ReadingListPlus.Common.Enums;

namespace ReadingListPlus.Services
{
    public interface ISchedulerService
    {
        void ChangeFirstCardPosition(IDeck deck, IEnumerable<ICard> cards, ICard card, Priority priority);

        Priority ParsePriority(string text);

        void PrepareForAdding(IDeck deck, IEnumerable<ICard> cards, ICard card, Priority priority);

        void PrepareForDeletion(IEnumerable<ICard> cards, ICard card);
    }
}