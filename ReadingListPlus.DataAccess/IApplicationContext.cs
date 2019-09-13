using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReadingListPlus.DataAccess.Models;

namespace ReadingListPlus.DataAccess
{
    public interface IApplicationContext
    {
        DbSet<Card> Cards { get; set; }

        DbSet<Deck> Decks { get; set; }

        DbSet<ApplicationUser> Users { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}