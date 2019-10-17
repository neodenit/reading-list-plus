using System;
using System.Threading.Tasks;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Services
{
    public interface ITextActionService
    {
        Task<CreateCardViewModel> ExtractAsync(Guid id, string text, string userName);

        Task<ReadCardViewModel> BookmarkAsync(Guid id, string text);

        Task<Uri> RememberAsync(Guid id, string text);

        Task<Guid> CancelRepetitionCardCreationAsync(Guid id);

        Task<Guid> CompleteRepetitionCardCreationAsync(Guid id);

        Task<Guid> HighlightAsync(Guid id, string selection);

        Task<Guid> ClozeAsync(Guid id, string selection);

        Task<Guid> DeleteRegionAsync(Guid id, string text);
    }
}