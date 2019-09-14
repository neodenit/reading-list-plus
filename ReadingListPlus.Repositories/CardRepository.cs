using System;
using System.Linq;
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
            Cards.Include(c => c.Deck.Cards).SingleOrDefaultAsync(c => c.ID == id);

        public Card GetCard(Guid id) =>
            Cards.Include(c => c.Deck.Cards).SingleOrDefault(c => c.ID == id);

        public void RemoveAll()
        {
            context.Cards.RemoveRange(context.Cards);
        }
    }
}
