using System;
using ReadingListPlus.Common;
using ReadingListPlus.Common.Enums;
using ReadingListPlus.DataAccess.Models;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Services
{
    public class MappingService : IMappingService
    {
        private readonly ISettings settings;
        private readonly ITextConverterService textConverterService;

        public MappingService(ISettings settings, ITextConverterService textConverterService)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.textConverterService = textConverterService ?? throw new ArgumentNullException(nameof(textConverterService));
        }

        public CardViewModel MapCardToViewModel(Card card) =>
            new CardViewModel
            {
                ID = card.ID,
                DeckID = card.DeckID,
                DeckTitle = card.Deck.Title,
                Type = card.CardType,
                Title = card.Title,
                Text = card.Text,
                Url = card.Url,
                Position = card.Position
            };

        public EditCardViewModel MapCardToEditViewModel(Card card) =>
            new EditCardViewModel
            {
                ID = card.ID,
                DeckID = card.DeckID,
                DeckTitle = card.Deck.Title,
                Type = card.CardType,
                Title = card.Title,
                Text = card.Text,
                Url = card.Url
            };

        public ReadCardViewModel MapCardToHtmlViewModel(Card card, NewRepetitionCardState newRepetitionCardState)
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
                Type = card.CardType,
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
    }
}
