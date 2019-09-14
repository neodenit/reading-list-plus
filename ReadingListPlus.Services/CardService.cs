using System;
using System.Threading.Tasks;
using ReadingListPlus.DataAccess.Models;
using ReadingListPlus.Repositories;

namespace ReadingListPlus.Services
{
    public class CardService : ICardService
    {
        private readonly ICardRepository repository;

        public CardService(ICardRepository repository)
        {
            this.repository = repository ?? throw new System.ArgumentNullException(nameof(repository));
        }

        public Task<Card> GetCardAsync(Guid id) =>
            repository.GetCardAsync(id);

        public async Task AddAsync(Card card)
        {
            await repository.AddAsync(card);
            await repository.SaveChangesAsync();
        }

        public Task RemoveAsync(Card card)
        {
            repository.Remove(card);
            return repository.SaveChangesAsync();
        }
    }
}
