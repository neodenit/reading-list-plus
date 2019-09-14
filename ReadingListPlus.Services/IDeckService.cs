using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReadingListPlus.DataAccess.Models;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Services
{
    public interface IDeckService
    {
        IAsyncEnumerable<DeckViewModel> GetUserDecks(string userName);

        Task<Deck> GetDeckAsync(Guid id);

        Task<DeckViewModel> GetDeckViewModelAsync(Guid id);

        Task<string> GetExportDataAsync();

        Task ImportAsync(ImportViewModel model, bool resetKeysOnImport);

        Task<Guid> GetFirstCardIdOrDefaultAsync(Guid deckId);

        Task CreateDeckAsync(string title, string userName);

        Task UpdateDeckTitleAsync(Guid id, string title);

        Task DeleteDeckAsync(Guid id);

        Guid? GetUserLastDeck(string userName);

        Task SetUserLastDeckAsync(string userName, Guid deckId);

        Task SaveChangesAsync();
    }
}