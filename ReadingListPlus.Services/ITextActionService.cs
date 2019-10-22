using System;
using System.Threading.Tasks;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Services
{
    public interface ITextActionService
    {
        Task<CreateCardViewModel> ExtractAsync(Guid id, string text, string userName);

        Task BookmarkAsync(Guid id, string text);

        Task HighlightAsync(Guid id, string selection);

        Task ClozeAsync(Guid id, string selection);

        Task DeleteRegionAsync(Guid id, string text);

        Task<Uri> RememberAsync(Guid id, string text);

        Task CancelRepetitionCardCreationAsync(Guid id);

        Task CompleteRepetitionCardCreationAsync(Guid id);
    }
}