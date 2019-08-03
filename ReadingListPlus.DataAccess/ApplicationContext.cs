using System.Linq;
using System.Security.Principal;
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

        public IQueryable<Deck> GetUserDecks(IPrincipal user)
        {
            var userName = user.Identity.Name;
            var items = Decks.Where(item => item.OwnerID == userName);

            return items;
        }
    }
}