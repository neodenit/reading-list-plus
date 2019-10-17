using ReadingListPlus.Common.Enums;
using ReadingListPlus.DataAccess.Models;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Services
{
    public interface IMappingService
    {
        CardViewModel MapCardToViewModel(Card card);

        EditCardViewModel MapCardToEditViewModel(Card card);

        ReadCardViewModel MapCardToHtmlViewModel(Card card, NewRepetitionCardState newRepetitionCardState);
    }
}