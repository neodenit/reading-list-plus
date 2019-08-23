using Microsoft.EntityFrameworkCore;
using ReadingListPlus.DataAccess;
using ReadingListPlus.DataAccess.Models;
using System;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace ReadingListPlus.Repositories
{
    public class DeckRepository : IDeckRepository
    {
        private readonly IApplicationContext context;

        public DeckRepository(IApplicationContext context)
        {
            this.context = context ?? throw new System.ArgumentNullException(nameof(context));
        }

        public DbSet<Deck> Decks => context.Decks;

        public DbSet<ApplicationUser> Users => context.Users;

        public Task<Deck> GetDeckAsync(Guid id) =>
            context.GetDeckAsync(id);

        public IQueryable<Deck> GetUserDecks(IPrincipal user) =>
            context.GetUserDecks(user);

        public Task<int> SaveChangesAsync() =>
            context.SaveChangesAsync();
    }
}
