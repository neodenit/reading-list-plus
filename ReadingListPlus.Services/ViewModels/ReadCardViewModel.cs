using System;
using ReadingListPlus.Common;
using ReadingListPlus.Common.Enums;
using ReadingListPlus.Services.Attributes;

namespace ReadingListPlus.Services.ViewModels
{
    public class ReadCardViewModel
    {
        [CardFound]
        [CardOwned]
        public Guid ID { get; set; }

        [DeckFound]
        [DeckOwned]
        public Guid? DeckID { get; set; }

        public Guid? ParentCardID { get; set; }

        public CardType CardType { get; set; }

        public string DeckTitle { get; set; }

        public string Title { get; set; }

        public string Text { get; set; }

        public string HtmlText { get; set; }

        public string Url { get; set; }

        public int Position { get; set; }

        public bool IsConnected => Position != Constants.DisconnectedCardPosition;

        public string Selection { get; set; }

        public CardAction NextAction { get; set; }

        public Priority? Priority { get; set; }

        public NewRepetitionCardState NewRepetitionCardState { get; set; }
    }
}
