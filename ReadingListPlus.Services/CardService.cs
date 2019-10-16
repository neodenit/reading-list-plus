using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReadingListPlus.Common;
using ReadingListPlus.Common.App_GlobalResources;
using ReadingListPlus.Common.Enums;
using ReadingListPlus.DataAccess.Models;
using ReadingListPlus.Repositories;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Services
{
    public class CardService : ICardService
    {
        private readonly ISettings settings;
        private readonly ICardRepository cardRepository;
        private readonly IDeckRepository deckRepository;
        private readonly ITextConverterService textConverterService;
        private readonly IRepetitionCardService repetitionCardService;
        private readonly ISchedulerService schedulerService;

        public CardService(ISettings settings, ICardRepository cardRepository, IDeckRepository deckRepository, ITextConverterService textConverterService, IRepetitionCardService repetitionCardService, ISchedulerService schedulerService)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.cardRepository = cardRepository ?? throw new System.ArgumentNullException(nameof(cardRepository));
            this.deckRepository = deckRepository ?? throw new ArgumentNullException(nameof(deckRepository));
            this.textConverterService = textConverterService ?? throw new ArgumentNullException(nameof(textConverterService));
            this.repetitionCardService = repetitionCardService ?? throw new ArgumentNullException(nameof(repetitionCardService));
            this.schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
        }

        public async Task<CardViewModel> GetCardAsync(Guid id)
        {
            Card card = await cardRepository.GetCardAsync(id);
            CardViewModel viewModel = MapCardToViewModel(card);
            return viewModel;
        }

        public async Task<EditCardViewModel> GetCardForEditingAsync(Guid id)
        {
            Card card = await cardRepository.GetCardAsync(id);
            EditCardViewModel viewModel = MapCardToEditViewModel(card);
            return viewModel;
        }

        public async Task<ReadCardViewModel> GetCardForReadingAsync(Guid id)
        {
            Card card = await cardRepository.GetCardAsync(id);

            string newRepetitionCardText = textConverterService.GetNewRepetitionCardText(card.Text);

            if (string.IsNullOrEmpty(newRepetitionCardText))
            {
                ReadCardViewModel viewModel = MapCardToHtmlViewModel(card, NewRepetitionCardState.None);
                return viewModel;
            }
            else
            {
                string reservedId = textConverterService.GetIdParameter(card.Text, Constants.NewRepetitionCardLabel);

                bool isValid = await repetitionCardService.IsRemoteIdValidAsync(card.ID, new Guid(reservedId));

                var newRepetitionCardState = isValid ? NewRepetitionCardState.Done : NewRepetitionCardState.Pending;

                ReadCardViewModel viewModel = MapCardToHtmlViewModel(card, newRepetitionCardState);
                return viewModel;
            }
        }

        public async Task<IEnumerable<CardViewModel>> GetAllCardsAsync(Guid deckId)
        {
            Deck deck = await deckRepository.GetDeckAsync(deckId);
            IEnumerable<CardViewModel> result = deck.Cards.Select(c => MapCardToViewModel(c));
            return result;
        }

        public async Task<IEnumerable<CardViewModel>> GetConnectedCardsAsync(Guid deckId)
        {
            Deck deck = await deckRepository.GetDeckAsync(deckId);
            IEnumerable<CardViewModel> result = deck.ConnectedCards.Select(c => MapCardToViewModel(c));
            return result;
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

            ReadCardViewModel viewModel = MapCardToHtmlViewModel(card, NewRepetitionCardState.None);

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

        public async Task<CardViewModel> PostponeAsync(Guid id, Priority priority)
        {
            Card card = await cardRepository.GetCardAsync(id);

            if (card.Position != Constants.FirstCardPosition)
            {
                CardViewModel viewModel = MapCardToViewModel(card);
                return viewModel;
            }
            else
            {
                schedulerService.ChangeFirstCardPosition(card, priority);

                await cardRepository.SaveChangesAsync();

                CardViewModel viewModel = MapCardToViewModel(card);
                return viewModel;
            }
        }

        public IEnumerable<KeyValuePair<string, string>> GetFullPriorityList()
        {
            var priorities = new[]
            {
                Priority.Highest,
                Priority.High,
                Priority.Medium,
                Priority.Low
            };

            return GetPriorities(priorities);
        }

        public IEnumerable<KeyValuePair<string, string>> GetShortPriorityList()
        {
            var priorities = new[]
            {
                Priority.High,
                Priority.Medium,
                Priority.Low
            };

            return GetPriorities(priorities);
        }

        public async Task<Guid> AddAsync(CreateCardViewModel card)
        {
            Deck deck = await deckRepository.GetDeckAsync(card.DeckID.Value);
            Deck oldDeck = card.OldDeckID.HasValue ? await deckRepository.GetDeckAsync(card.OldDeckID.Value) : null;
            Card parentCard = card.ParentCardID.HasValue ? await cardRepository.GetCardAsync(card.ParentCardID.Value) : null;

            var priority = card.Priority.Value;

            var newCard = new Card
            {
                ID = Guid.NewGuid(),
                DeckID = card.DeckID,
                Title = card.Title,
                Text = card.Text,
                Url = card.Url,
                Type = card.Type,
                ParentCardID = card.ParentCardID,
            };

            schedulerService.PrepareForAdding(deck, newCard, priority);

            await cardRepository.AddAsync(newCard);

            if (oldDeck != null)
            {
                oldDeck.DependentDeckID = newCard.DeckID;
            }

            if (parentCard != null)
            {
                await cardRepository.SaveChangesAsync();

                string textWithNewCardID = textConverterService.AddParameter(card.ParentCardUpdatedText, Constants.SelectionLabel, newCard.ID.ToString());
                string textWithoutSelection = textConverterService.ReplaceTag(textWithNewCardID, Constants.SelectionLabel, Constants.ExtractLabel);

                parentCard.Text = textWithoutSelection;
            }

            await cardRepository.SaveChangesAsync();

            return newCard.DeckID.Value;
        }

        public async Task<CardViewModel> UpdateAsync(EditCardViewModel card)
        {
            Card dbCard = await cardRepository.GetCardAsync(card.ID);

            dbCard.Title = card.Title;
            dbCard.Text = card.Text;
            dbCard.Url = card.Url;
            dbCard.Type = card.Type;

            await cardRepository.SaveChangesAsync();

            CardViewModel viewModel = MapCardToViewModel(dbCard);
            return viewModel;
        }

        public async Task<CardViewModel> HideCardAsync(Guid id)
        {
            Card card = await cardRepository.GetCardAsync(id);

            schedulerService.PrepareForDeletion(card.Deck, card);

            card.Position = Constants.DisconnectedCardPosition;

            await cardRepository.SaveChangesAsync();

            CardViewModel viewModel = MapCardToViewModel(card);
            return viewModel;
        }

        public async Task RestoreAsync(CardViewModel card)
        {
            Card dbCard = await cardRepository.GetCardAsync(card.ID);
            Deck dbDeck = await deckRepository.GetDeckAsync(card.DeckID.Value);

            if (dbCard.IsConnected)
            {
                throw new InvalidOperationException();
            }

            if (dbCard.DeckID == null)
            {
                dbCard.DeckID = card.DeckID;
            }

            schedulerService.PrepareForAdding(dbDeck, dbCard, card.Priority.Value);

            await cardRepository.SaveChangesAsync();
        }

        public async Task RemoveAsync(Guid id)
        {
            Card card = await cardRepository.GetCardAsync(id);

            if (card.IsConnected)
            {
                throw new InvalidOperationException();
            }

            if (card.ParentCardID.HasValue)
            {
                Card parentCard = await cardRepository.GetCardAsync(card.ParentCardID.Value);
                string text = textConverterService.DeleteTagByNameAndParam(parentCard.Text, Constants.ExtractLabel, card.ID);
                parentCard.Text = text;
            }

            cardRepository.Remove(card);

            await cardRepository.SaveChangesAsync();
        }

        private CardViewModel MapCardToViewModel(Card card) =>
            new CardViewModel
            {
                ID = card.ID,
                DeckID = card.DeckID,
                DeckTitle = card.Deck.Title,
                Type = card.Type,
                Title = card.Title,
                Text = card.Text,
                Url = card.Url,
                Position = card.Position
            };

        private ReadCardViewModel MapCardToHtmlViewModel(Card card, NewRepetitionCardState newRepetitionCardState)
        {
            var cardUrlTemplate = $"/Cards/Read/${{{Constants.IdGroup}}}";

            var repetitionCardUrlTemplate = new Uri(new Uri(settings.SpacedRepetitionServer), $"Cards/Edit/{textConverterService.GetIdParameter(card.Text, Constants.RepetitionCardLabel)}").AbsoluteUri;

            var newRepetitionCardUrlTemplate = newRepetitionCardState == NewRepetitionCardState.Done
                ? new Uri(new Uri(settings.SpacedRepetitionServer), $"Cards/Edit/{textConverterService.GetIdParameter(card.Text, Constants.NewRepetitionCardLabel)}").AbsoluteUri
                : new Uri(new Uri(settings.SpacedRepetitionServer), $"Cards/Create?readingCardId={card.ID}&repetitionCardId={textConverterService.GetIdParameter(card.Text, Constants.NewRepetitionCardLabel)}&text={Uri.EscapeDataString(textConverterService.GetNewRepetitionCardText(card.Text))}").AbsoluteUri;

            var newRepetitionCardClass = newRepetitionCardState == NewRepetitionCardState.Done
                ? Constants.RepetitionCardLabel
                : Constants.NewRepetitionCardLabel;

            var model = new ReadCardViewModel
            {
                ID = card.ID,
                DeckID = card.DeckID,
                ParentCardID = card.ParentCardID,
                DeckTitle = card.Deck.Title,
                Type = card.Type,
                Title = card.Title,
                Text = card.Text,
                HtmlText = textConverterService.GetHtml(
                    card.Text,
                    cardUrlTemplate,
                    repetitionCardUrlTemplate,
                    newRepetitionCardUrlTemplate,
                    newRepetitionCardClass),
                Url = card.Url,
                Position = card.Position,
                NewRepetitionCardState = newRepetitionCardState
            };

            return model;
        }

        private EditCardViewModel MapCardToEditViewModel(Card card) =>
            new EditCardViewModel
            {
                ID = card.ID,
                DeckID = card.DeckID,
                DeckTitle = card.Deck.Title,
                Type = card.Type,
                Title = card.Title,
                Text = card.Text,
                Url = card.Url
            };

        private IEnumerable<KeyValuePair<string, string>> GetPriorities(IEnumerable<Priority> priorities) =>
            priorities.Select(p => new KeyValuePair<string, string>(p.ToString("d"), GetPriorityLabel(p)));

        private string GetPriorityLabel(Priority priority)
        {
            switch (priority)
            {
                case Priority.Highest:
                    return Resources.HighestPriorityWithWarning;
                case Priority.High:
                    return Resources.HighPriority;
                case Priority.Medium:
                    return Resources.MediumPriority;
                case Priority.Low:
                    return Resources.LowPriority;
                default:
                    return string.Empty;
            }
        }
    }
}
