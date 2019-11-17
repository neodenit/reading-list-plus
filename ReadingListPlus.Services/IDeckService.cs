using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Services
{
    public interface IDeckService
    {
        Task<IEnumerable<DeckViewModel>> GetUserDecksAsync(string userName);

        Task<DeckViewModel> GetDeckAsync(Guid id);

        Task<string> GetExportDataAsync();

        Task ImportAsync(Stream stream, bool resetKeysOnImport, bool fixOnImport);

        Task<Guid> GetFirstCardIdOrDefaultAsync(Guid deckId);

        Task CreateDeckAsync(string title, string userName);

        Task UpdateDeckTitleAsync(Guid id, string title);

        Task DeleteDeckAsync(Guid id);

        Guid? GetUserLastDeck(string userName);

        Task SetUserLastDeckAsync(string userName, Guid deckId);

        Task FixDeckAsync(Guid deckId);
    }
}