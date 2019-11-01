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

        public TextActionService(ISettings settings, ICardRepository cardRepository, ITextConverterService textConverterService)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.cardRepository = cardRepository ?? throw new System.ArgumentNullException(nameof(cardRepository));
            this.textConverterService = textConverterService ?? throw new ArgumentNullException(nameof(textConverterService));
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
                CardType = CardType.Extract,
                CreationMode = CreationMode.Extract
            };

            newCard.DeckID = card.Deck?.DependentDeckID;

            return newCard;
        }

        public async Task BookmarkAsync(Guid id, string text)
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

        public async Task CancelRepetitionCardCreationAsync(Guid id)
        {
            if (!settings.RememberEnabled)
            {
                throw new InvalidOperationException();
            }

            Card card = await cardRepository.GetCardAsync(id);

            string newText = textConverterService.DeleteTagByName(card.Text, Constants.NewRepetitionCardLabel);

            card.Text = newText;

            await cardRepository.SaveChangesAsync();
        }

        public async Task CompleteRepetitionCardCreationAsync(Guid id)
        {
            if (!settings.RememberEnabled)
            {
                throw new InvalidOperationException();
            }

            Card card = await cardRepository.GetCardAsync(id);

            string newText = textConverterService.ReplaceTag(card.Text, Constants.NewRepetitionCardLabel, Constants.RepetitionCardLabel);

            card.Text = newText;

            await cardRepository.SaveChangesAsync();
        }

        public async Task HighlightAsync(Guid id, string text)
        {
            if (!settings.HighlightEnabled)
            {
                throw new InvalidOperationException();
            }

            Card card = await cardRepository.GetCardAsync(id);

            string pattern = textConverterService.GetPatternForSelection(text);
            string newText = textConverterService.AddHighlight(card.Text, pattern);

            card.Text = newText;

            await cardRepository.SaveChangesAsync();
        }

        public async Task ClozeAsync(Guid id, string text)
        {
            if (!settings.ClozeEnabled)
            {
                throw new InvalidOperationException();
            }

            Card card = await cardRepository.GetCardAsync(id);

            string pattern = textConverterService.GetPatternForSelection(text);
            string newText = textConverterService.AddCloze(card.Text, pattern);

            card.Text = newText;

            await cardRepository.SaveChangesAsync();
        }

        public async Task DeleteRegionAsync(Guid id, string text)
        {
            if (!settings.DropEnabled)
            {
                throw new InvalidOperationException();
            }

            Card card = await cardRepository.GetCardAsync(id);

            string pattern = textConverterService.GetPatternForDeletion(text);
            string newText = textConverterService.DeleteTagByText(card.Text, pattern);

            card.Text = newText;

            await cardRepository.SaveChangesAsync();
        }
    }
}
