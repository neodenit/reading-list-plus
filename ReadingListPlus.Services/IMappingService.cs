using ReadingListPlus.Common.Enums;
using ReadingListPlus.DataAccess.Models;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Services
{
    public interface IMappingService
    {
        ReadCardViewModel MapCardToHtmlViewModel(Card card, NewRepetitionCardState newRepetitionCardState);
    }
}