using System;
using System.Threading.Tasks;

namespace ReadingListPlus.Services
{
    public interface IRepetitionCardService
    {
        Task<bool> IsActionValidAsync(Guid id, string action);

        Task<bool> IsLocalIdValidAsync(Guid readingCardId, Guid repetitionCardId);

        Task<bool> IsRemoteIdValidAsync(Guid readingCardId, Guid repetitionCardId);
    }
}