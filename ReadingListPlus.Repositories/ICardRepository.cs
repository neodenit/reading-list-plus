using System;
using System.Threading.Tasks;
using ReadingListPlus.DataAccess.Models;

namespace ReadingListPlus.Repositories
{
    public interface ICardRepository
    {
        Task<Card> GetCardAsync(Guid id);

        Card GetCard(Guid id);

        Task AddAsync(Card card);

        void RemoveAll();

        void Remove(Card card);

        Task SaveChangesAsync();
    }
}