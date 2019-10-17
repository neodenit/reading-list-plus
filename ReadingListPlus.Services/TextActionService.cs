using System;
using System.Threading.Tasks;
using ReadingListPlus.Common;
using ReadingListPlus.Common.Enums;
using ReadingListPlus.DataAccess.Models;
using ReadingListPlus.Repositories;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Services
{
    public class TextActionService : ITextActionService
    {
        private readonly ISettings settings;
        private readonly ICardRepository cardRepository;
        private readonly ITextConverterService textConverterService;
        private readonly IMappingService mappingService;

        public TextActionService(ISettings settings, ICardRepository cardRepository, ITextConverterService textConverterService, IMappingService mappingService)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.cardRepository = cardRepository ?? throw new System.ArgumentNullException(nameof(cardRepository));
            this.textConverterService = textConverterService ?? throw new ArgumentNullException(nameof(textConverterService));
            this.mappingService = mappingService ?? throw new ArgumentNullException(nameof(mappingService));
        }

        public async Task<CreateCardViewModel> ExtractAsync(Guid id, string text, string userName)
        {
            if (!settings.ExtractEnabled)
            {
                throw new InvalidOperationException();
            }

            Card card = await cardRepository.GetCardAsync(id);

            string selection = textConverterService.GetSelection(text);

            var newCard = new CreateCardViewModel
            {
                Url = card.Url,
                ParentCardID = card.ID,
                OldDeckID = card.DeckID,
                Text = selection,
                ParentCardUpdatedText = text,
                Type = CardType.Extract,
                CreationMode = CreationMode.Extract
            };

            newCard.DeckID = card.Deck?.DependentDeckID;

            return newCard;
        }

        public async Task<ReadCardViewModel> BookmarkAsync(Guid id, string text)
        {
            if (!settings.BookmarkEnabled)
            {
                throw new InvalidOperationException();
            }

            Card card = await cardRepository.GetCardAsync(id);

            string textWithoutBookmarks = textConverterService.DeleteTagByName(text, "bookmark");

            string newText = textConverterService.ReplaceTag(textWithoutBookmarks, Constants.SelectionLabel, "bookmark");

            card.Text = newText;

            await cardRepository.SaveChangesAsync();

            ReadCardViewModel viewModel = mappingService.MapCardToHtmlViewModel(card, NewRepetitionCardState.None);

            viewModel.IsBookmarked = true;

            return viewModel;
        }

        public async Task<Uri> RememberAsync(Guid cardId, string text)
        {
            if (!settings.RememberEnabled)
            {
                throw new InvalidOperationException();
            }

            Card card = await cardRepository.GetCardAsync(cardId);

            string repetitionCardText = textConverterService.GetSelection(text);
            var encodedRepetitionCardText = Uri.EscapeDataString(repetitionCardText);

            string textWithNewRepetitionCard = textConverterService.ReplaceTag(text, Constants.SelectionLabel, Constants.NewRepetitionCardLabel);
            var repetitionCardId = Guid.NewGuid().ToString();
            string textWithNewRepetitionCardId = textConverterService.AddParameter(textWithNewRepetitionCard, Constants.NewRepetitionCardLabel, repetitionCardId);

            card.Text = textWithNewRepetitionCardId;

            await cardRepository.SaveChangesAsync();

            var baseUri = new Uri(settings.SpacedRepetitionServer);
            var uri = new Uri(baseUri, $"Cards/Create?readingCardId={cardId}&repetitionCardId={repetitionCardId}&text={encodedRepetitionCardText}");
            return uri;
        }

        public async Task<Guid> CancelRepetitionCardCreationAsync(Guid id)
        {
            if (!settings.RememberEnabled)
            {
                throw new InvalidOperationException();
            }

            Card card = await cardRepository.GetCardAsync(id);

            string newText = textConverterService.DeleteTagByName(card.Text, Constants.NewRepetitionCardLabel);

            card.Text = newText;

            await cardRepository.SaveChangesAsync();

            return card.ID;
        }

        public async Task<Guid> CompleteRepetitionCardCreationAsync(Guid id)
        {
            if (!settings.RememberEnabled)
            {
                throw new InvalidOperationException();
            }

            Card card = await cardRepository.GetCardAsync(id);

            string newText = textConverterService.ReplaceTag(card.Text, Constants.NewRepetitionCardLabel, Constants.RepetitionCardLabel);

            card.Text = newText;

            await cardRepository.SaveChangesAsync();

            return card.ID;
        }

        public async Task<Guid> HighlightAsync(Guid id, string text)
        {
            if (!settings.HighlightEnabled)
            {
                throw new InvalidOperationException();
            }

            Card card = await cardRepository.GetCardAsync(id);

            string newText = textConverterService.AddHighlight(card.Text, text);

            card.Text = newText;

            await cardRepository.SaveChangesAsync();

            return card.ID;
        }

        public async Task<Guid> ClozeAsync(Guid id, string text)
        {
            if (!settings.ClozeEnabled)
            {
                throw new InvalidOperationException();
            }

            Card card = await cardRepository.GetCardAsync(id);

            string newText = textConverterService.AddCloze(card.Text, text);

            card.Text = newText;

            await cardRepository.SaveChangesAsync();

            return card.ID;
        }

        public async Task<Guid> DeleteRegionAsync(Guid id, string text)
        {
            if (!settings.DropEnabled)
            {
                throw new InvalidOperationException();
            }

            Card card = await cardRepository.GetCardAsync(id);

            string newText = textConverterService.DeleteTagByText(card.Text, text);

            card.Text = newText;

            await cardRepository.SaveChangesAsync();

            return card.ID;
        }
    }
}
