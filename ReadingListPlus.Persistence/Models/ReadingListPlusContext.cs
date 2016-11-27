using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Linq;
using System.Security.Principal;

namespace ReadingListPlus.Persistence.Models
{
    public class ReadingListPlusContext : IdentityDbContext<ApplicationUser>
    {
        public ReadingListPlusContext() : base("DefaultConnection") { }

        public static ReadingListPlusContext Create()
        {
            return new ReadingListPlusContext();
        }
        
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