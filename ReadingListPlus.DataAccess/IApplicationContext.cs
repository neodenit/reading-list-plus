using System;
using System.Linq;
using System.Security.Principal;
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

        Task<Card> GetCardAsync(Guid id);

        Task<Deck> GetDeckAsync(Guid id);

        Deck GetDeck(Guid id);

        IQueryable<Deck> GetUserDecks(IPrincipal user);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}