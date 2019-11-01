using System;
using System.ComponentModel.DataAnnotations;
using ReadingListPlus.Common;
using ReadingListPlus.Common.Enums;
using ReadingListPlus.Services.Attributes;

namespace ReadingListPlus.Services.ViewModels
{
    public class CardViewModel
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

        public string DisplayText => string.IsNullOrEmpty(Title) ? Text : $"{Title} - {Text}";

        public string Url { get; set; }

        public int Position { get; set; }

        public bool IsConnected => Position != Constants.DisconnectedCardPosition;

        [Display(Name = "#")]
        [DisplayFormat(NullDisplayText = "-")]
        public int? DisplayPosition => IsConnected
            ? Position + 1
            : null as int?;
    }
}
