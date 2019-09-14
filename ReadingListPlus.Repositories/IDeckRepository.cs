using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReadingListPlus.DataAccess.Models;

namespace ReadingListPlus.Repositories
{
    public interface IDeckRepository
    {
        Task<Deck> GetDeckAsync(Guid id);

        IAsyncEnumerable<Deck> GetAllDecks();

        IAsyncEnumerable<Deck> GetUserDecks(string userName);

        Deck GetDeck(Guid id);

        void AddRange(IEnumerable<Deck> newDecks);

        void Add(Deck deck);

        void RemoveAll();

        void Remove(Deck deck);

        Task SaveChangesAsync();
    }
}