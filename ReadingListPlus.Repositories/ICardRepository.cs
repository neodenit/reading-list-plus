using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReadingListPlus.DataAccess.Models;

namespace ReadingListPlus.Repositories
{
    public interface ICardRepository
    {
        IAsyncEnumerable<Card> GetAllCards();

        IAsyncEnumerable<Card> GetUnparentedCards(string userName);

        Task<Card> GetCardAsync(Guid id);

        Card GetCard(Guid id);

        Task AddAsync(Card card);

        void RemoveAll();

        void Remove(Card card);

        Task SaveChangesAsync();
    }
}