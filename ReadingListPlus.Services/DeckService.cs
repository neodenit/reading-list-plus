using Microsoft.EntityFrameworkCore;
using ReadingListPlus.DataAccess;
using ReadingListPlus.DataAccess.Models;
using ReadingListPlus.Repositories;
using System;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace ReadingListPlus.Services
{
    public class DeckService : IDeckService
    {
        private readonly IDeckRepository repository;

        public DeckService(IDeckRepository repository)
        {
            this.repository = repository ?? throw new System.ArgumentNullException(nameof(repository));
        }

        public DbSet<Deck> Decks => repository.Decks;

        public DbSet<ApplicationUser> Users => repository.Users;

        public Task<Deck> GetDeckAsync(Guid id) =>
            repository.GetDeckAsync(id);

        public IQueryable<Deck> GetUserDecks(IPrincipal user) =>
            repository.GetUserDecks(user);

        public Task<int> SaveChangesAsync() =>
            repository.SaveChangesAsync();
    }
}
