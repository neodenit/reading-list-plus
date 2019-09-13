using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReadingListPlus.Common.Interfaces;
using ReadingListPlus.DataAccess;
using ReadingListPlus.DataAccess.Models;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Services
{
    public interface IDeckService
    {
        DbSet<Deck> Decks { get; }

        DbSet<ApplicationUser> Users { get; }

        IAsyncEnumerable<DeckViewModel> GetUserDecks(string userName);

        Task<Deck> GetDeckAsync(Guid id);

        Task<DeckViewModel> GetDeckViewModelAsync(Guid id);

        Task<string> GetExportDataAsync();

        Task ImportAsync(ImportViewModel model, bool resetKeysOnImport);

        Task<ICard> GetFirstCardOrDefaultAsync(Guid deckId);

        Task CreateDeckAsync(string title, string userName);

        Task UpdateDeckTitleAsync(Guid id, string title);

        Task DeleteDeckAsync(Guid id);

        Task<int> SaveChangesAsync();
    }
}