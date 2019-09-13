using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ReadingListPlus.DataAccess.Models;

namespace ReadingListPlus.DataAccess
{
    public class ApplicationContext : IdentityDbContext<ApplicationUser>, IApplicationContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }

        public DbSet<Deck> Decks { get; set; }

        public DbSet<Card> Cards { get; set; }
    }
}