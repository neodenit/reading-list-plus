using System;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReadingListPlus.DataAccess;
using ReadingListPlus.DataAccess.Models;

namespace ReadingListPlus.Repositories
{
    public interface IDeckRepository
    {
        DbSet<Deck> Decks { get; }

        DbSet<ApplicationUser> Users { get; }

        Task<Deck> GetDeckAsync(Guid id);

        IQueryable<Deck> GetUserDecks(IPrincipal user);

        Task<int> SaveChangesAsync();
    }
}