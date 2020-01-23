using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
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
        private readonly IMapper mapper;
        private readonly ICardRepository cardRepository;
        private readonly IDeckRepository deckRepository;
        private readonly ITextConverterService textConverterService;
        private readonly IRepetitionCardService repetitionCardService;
        private readonly ISchedulerService schedulerService;
        private readonly IMappingService mappingService;

        public CardService(IMapper mapper, ICardRepository cardRepository, IDeckRepository deckRepository, ITextConverterService textConverterService, IRepetitionCardService repetitionCardService, ISchedulerService schedulerService, IMappingService mappingService)
        {
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.cardRepository = cardRepository ?? throw new System.ArgumentNullException(nameof(cardRepository));
            this.deckRepository = deckRepository ?? throw new ArgumentNullException(nameof(deckRepository));
            this.textConverterService = textConverterService ?? throw new ArgumentNullException(nameof(textConverterService));
            this.repetitionCardService = repetitionCardService ?? throw new ArgumentNullException(nameof(repetitionCardService));
            this.schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
            this.mappingService = mappingService ?? throw new ArgumentNullException(nameof(mappingService));
        }

        public async Task<CardViewModel> GetCardAsync(Guid id)
        {
            Card card = await cardRepository.GetCardAsync(id);
            var viewModel = mapper.Map<CardViewModel>(card);
            return viewModel;
        }

        public async Task<EditCardViewModel> GetCardForEditingAsync(Guid id)
        {
            Card card = await cardRepository.GetCardAsync(id);
            var viewModel = mapper.Map<EditCardViewModel>(card);
            return viewModel;
        }

        public async Task<ReadCardViewModel> GetCardForReadingAsync(Guid id)
        {
            Card card = await cardRepository.GetCardAsync(id);

            string newRepetitionCardText = textConverterService.GetNewRepetitionCardText(card.Text);

            if (string.IsNullOrEmpty(newRepetitionCardText))
            {
                ReadCardViewModel viewModel = mappingService.MapCardToHtmlViewModel(card, NewRepetitionCardState.None);
                return viewModel;
            }
            else
            {
                string reservedId = textConverterService.GetIdParameter(card.Text, Constants.NewRepetitionCardLabel);

                bool isValid = await repetitionCardService.IsRemoteIdValidAsync(card.ID, new Guid(reservedId));

                var newRepetitionCardState = isValid ? NewRepetitionCardState.Done : NewRepetitionCardState.Pending;

                ReadCardViewModel viewModel = mappingService.MapCardToHtmlViewModel(card, newRepetitionCardState);
                return viewModel;
            }
        }

        public async Task<IEnumerable<CardViewModel>> GetAllCardsAsync(Guid deckId)
        {
            Deck deck = await deckRepository.GetDeckAsync(deckId);
            var viewModel = mapper.Map<IEnumerable<CardViewModel>>(deck.Cards);
            return viewModel;
        }

        public async IAsyncEnumerable<CardViewModel> GetUnparentedCardsAsync(string userName)
        {
            IAsyncEnumerable<Card> cards = cardRepository.GetUnparentedCards(userName);

            await foreach (var card in cards)
            {
                var viewModel = mapper.Map<CardViewModel>(card);
                yield return viewModel;
            }
        }


        public async Task<IEnumerable<CardViewModel>> GetConnectedCardsAsync(Guid deckId)
        {
            Deck deck = await deckRepository.GetDeckAsync(deckId);
            var viewModel = mapper.Map<IEnumerable<CardViewModel>>(deck.Cards);
            return viewModel;
        }

        public async Task<CardViewModel> PostponeAsync(Guid id, Priority priority)
        {
            Card card = await cardRepository.GetCardAsync(id);

            var initialCardNumber = card.Deck.Cards.Count();

            schedulerService.ChangeCardPosition(card, priority);

            ValidateCardNumber(initialCardNumber, card.Deck);
            ValidatePositions(card.Deck.ConnectedCards);

            await cardRepository.SaveChangesAsync();

            var viewModel = mapper.Map<CardViewModel>(card);
            return viewModel;
        }

        private void ValidateCardNumber(int initialCardNumber, Deck deck)
        {
            if (deck.Cards.Count != initialCardNumber)
            {
                throw new InvalidOperationException("Card number is incorrect.");
            }
        }

        private void ValidatePositions(IEnumerable<Card> cards)
        {
            var cardCount = cards.Count();
            var expectedPositions = Enumerable.Range(Constants.FirstCardPosition, cardCount);
            var actualPositions = cards.Select(c => c.Position).OrderBy(x => x);

            var isValid = actualPositions.SequenceEqual(expectedPositions);

            if (!isValid)
            {
                throw new InvalidOperationException("Card positions are incorrect.");
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

        public async Task<Guid> AddAsync(CreateCardViewModel card, string userName)
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
                CardType = card.CardType,
                ParentCardID = card.ParentCardID,
                OwnerID = userName
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

                string textWithoutLastExtract = textConverterService.ReplaceTag(textWithNewCardID, Constants.LastExtractLabel, Constants.ExtractLabel);

                string textWithoutSelection = textConverterService.ReplaceTag(textWithoutLastExtract, Constants.SelectionLabel, Constants.LastExtractLabel);

                string textWithoutBookmarks = textConverterService.DeleteTagByName(textWithoutSelection, Constants.BookmarkLabel);

                parentCard.Text = textWithoutBookmarks;
            }

            await cardRepository.SaveChangesAsync();

            return newCard.DeckID.Value;
        }

        public async Task UpdateAsync(EditCardViewModel card)
        {
            Card dbCard = await cardRepository.GetCardAsync(card.ID);

            dbCard.Title = card.Title;
            dbCard.Text = card.Text;
            dbCard.Url = card.Url;
            dbCard.CardType = card.CardType;

            await cardRepository.SaveChangesAsync();
        }

        public async Task HideCardAsync(Guid id)
        {
            Card card = await cardRepository.GetCardAsync(id);

            schedulerService.PrepareForDeletion(card);

            await cardRepository.SaveChangesAsync();
        }

        public async Task RestoreAsync(CardViewModel card, Priority priority)
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

            schedulerService.PrepareForAdding(dbDeck, dbCard, priority);

            await cardRepository.SaveChangesAsync();
        }

        public async Task MoveAsync(Guid cardId, Guid newDeckId, Priority priority)
        {
            Card dbCard = await cardRepository.GetCardAsync(cardId);
            Deck newDeck = await deckRepository.GetDeckAsync(newDeckId);

            schedulerService.PrepareForDeletion(dbCard);
            ValidatePositions(dbCard.Deck.ConnectedCards);

            schedulerService.PrepareForAdding(newDeck, dbCard, priority);
            ValidatePositions(newDeck.ConnectedCards.Concat(Enumerable.Repeat(dbCard, 1)));

            dbCard.DeckID = newDeck.ID;

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

        public IEnumerable<string> ValidateTagNames(string text)
        {
            IEnumerable<string> tagNames = textConverterService.GetTagNames(text);

            var invalidTagNames = tagNames.Where(n => !Constants.TagNames.Contains(n));
            return invalidTagNames;
        }

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

        public async Task FixCardOwnerAsync(string defaultOwner)
        {
            IAsyncEnumerable<Card> cards = cardRepository.GetAllCards();

            await foreach (var card in cards)
            {
                card.OwnerID = card.Deck?.OwnerID ?? defaultOwner;
            }

            await cardRepository.SaveChangesAsync();
        }

        public async Task FixAsync(FixAction action)
        {
            static async Task FixAmpersandsAsync(IAsyncEnumerable<Card> cards)
            {
                await foreach (var card in cards)
                {
                    card.Text = Regex.Replace(card.Text, "&(amp;)+", "&");
                }
            }

            static async Task FixSpacesAsync(IAsyncEnumerable<Card> cards)
            {
                await foreach (var card in cards)
                {
                    card.Text = Regex.Replace(card.Text, "&nbsp;", " ");
                }
            }

            IAsyncEnumerable<Card> cards = cardRepository.GetAllCards();

            switch (action)
            {
                case FixAction.FixAmpersands:
                    await FixAmpersandsAsync(cards);
                    break;
                case FixAction.FixSpaces:
                    await FixSpacesAsync(cards);
                    break;
            }

            await cardRepository.SaveChangesAsync();
        }
    }
}
