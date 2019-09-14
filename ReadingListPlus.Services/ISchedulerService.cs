using System.Collections.Generic;
using ReadingListPlus.Common.Enums;
using ReadingListPlus.DataAccess.Models;

namespace ReadingListPlus.Services
{
    public interface ISchedulerService
    {
        void ChangeFirstCardPosition(Deck deck, IEnumerable<Card> cards, Card card, Priority priority);

        Priority ParsePriority(string text);

        void PrepareForAdding(Deck deck, IEnumerable<Card> cards, Card card, Priority priority);

        void PrepareForDeletion(IEnumerable<Card> cards, Card card);

        Card GetFirstCard(IEnumerable<Card> cards);
    }
}