using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

        public DbSet<Card> Cards => repository.Cards;

        public Task<Card> GetCardAsync(Guid id) =>
            repository.GetCardAsync(id);
    }
}
