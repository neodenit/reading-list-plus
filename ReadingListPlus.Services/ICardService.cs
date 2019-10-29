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

        Task<ReadCardViewModel> GetCardForReadingAsync(Guid id);

        Task<IEnumerable<CardViewModel>> GetAllCardsAsync(Guid deckId);

        Task<IEnumerable<CardViewModel>> GetConnectedCardsAsync(Guid deckId);

        Task<CardViewModel> PostponeAsync(Guid id, Priority priority);

        IEnumerable<KeyValuePair<string, string>> GetFullPriorityList();

        IEnumerable<KeyValuePair<string, string>> GetShortPriorityList();

        Task<Guid> AddAsync(CreateCardViewModel card);

        Task<CardViewModel> UpdateAsync(EditCardViewModel card);

        Task<CardViewModel> HideCardAsync(Guid id);

        Task RestoreAsync(CardViewModel card, Priority priority);

        Task MoveAsync(Guid cardId, Guid newDeckId, Priority priority);

        Task RemoveAsync(Guid id);
    }
}