using System.Collections.Generic;
using ReadingListPlus.Common.Enums;
using ReadingListPlus.DataAccess.Models;

namespace ReadingListPlus.Services
{
    public interface ISchedulerService
    {
        void ChangeFirstCardPosition(Card card, Priority priority);

        Priority ParsePriority(string text);

        void PrepareForAdding(Deck deck, Card card, Priority priority);

        void PrepareForDeletion(Deck deck, Card card);

        Card GetFirstCard(Deck deck);
    }
}