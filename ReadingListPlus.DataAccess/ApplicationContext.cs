using System;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ReadingListPlus.DataAccess.Models;

namespace ReadingListPlus.DataAccess
{
    public class ApplicationContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }
        
        public DbSet<Deck> Decks { get; set; }

        public DbSet<Card> Cards { get; set; }

        public Task<Deck> GetDeckAsync(Guid id) =>
            Decks.Include(d => d.Cards).SingleOrDefaultAsync(d => d.ID == id);

        public Task<Card> GetCardAsync(Guid id) =>
            Cards.Include(c => c.Deck).SingleOrDefaultAsync(c => c.ID == id);

        public IQueryable<Deck> GetUserDecks(IPrincipal user)
        {
            var userName = user.Identity.Name;
            var items = Decks.Include(d => d.Cards).Where(item => item.OwnerID == userName);

            return items;
        }
    }
}