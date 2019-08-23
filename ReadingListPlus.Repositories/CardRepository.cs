using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReadingListPlus.DataAccess;
using ReadingListPlus.DataAccess.Models;

namespace ReadingListPlus.Repositories
{
    public class CardRepository : ICardRepository
    {
        private readonly IApplicationContext context;

        public CardRepository(IApplicationContext context)
        {
            this.context = context ?? throw new System.ArgumentNullException(nameof(context));
        }

        public DbSet<Card> Cards => context.Cards;

        public Task<Card> GetCardAsync(Guid id) =>
            context.GetCardAsync(id);
    }
}
