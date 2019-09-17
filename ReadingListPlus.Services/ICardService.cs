using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReadingListPlus.Common.Enums;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Services
{
    public interface ICardService
    {
        Task<CardViewModel> GetCardAsync(Guid id);

        Task<EditCardViewModel> GetCardForEditingAsync(Guid id);

        Task<CardViewModel> GetCardForReadingAsync(Guid id);

        Task<IEnumerable<CardViewModel>> GetConnectedCards(Guid deckId);

        Task<CreateCardViewModel> ExtractAsync(Guid id, string text, string userName);

        Task<CardViewModel> BookmarkAsync(Guid id, string text);

        Task<Uri> RememberAsync(Guid id, string text);

        Task<Guid> CancelRepetitionCardCreationAsync(Guid id);

        Task<Guid> CompleteRepetitionCardCreationAsync(Guid id);

        Task<Guid> HighlightAsync(Guid id, string selection);

        Task<Guid> ClozeAsync(Guid id, string selection);

        Task<Guid> DeleteRegionAsync(Guid id, string text);

        Task<CardViewModel> PostponeAsync(Guid id, Priority priority);

        IEnumerable<KeyValuePair<string, string>> GetFullPriorityList();

        IEnumerable<KeyValuePair<string, string>> GetShortPriorityList();

        Task<Guid> AddAsync(CreateCardViewModel card);

        Task<CardViewModel> UpdateAsync(EditCardViewModel card);

        Task<CardViewModel> HideCardAsync(Guid id);

        Task RemoveAsync(Guid id);
    }
}