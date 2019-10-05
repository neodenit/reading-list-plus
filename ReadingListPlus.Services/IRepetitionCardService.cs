using System;
using System.Threading.Tasks;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Services
{
    public interface IRepetitionCardService
    {
        Task<bool> IsActionValidAsync(Guid id, CardAction action);

        Task<bool> IsLocalIdValidAsync(Guid readingCardId, Guid repetitionCardId);

        Task<bool> IsRemoteIdValidAsync(Guid readingCardId, Guid repetitionCardId);
    }
}