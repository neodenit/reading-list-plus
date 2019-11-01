using System;
using System.ComponentModel.DataAnnotations;
using ReadingListPlus.Common.Enums;
using ReadingListPlus.Services.Attributes;

namespace ReadingListPlus.Services.ViewModels
{
    public class EditCardViewModel
    {
        [CardFound]
        [CardOwned]
        public Guid ID { get; set; }

        [DeckFound]
        [DeckOwned]
        public Guid? DeckID { get; set; }

        public string DeckTitle { get; set; }

        public CardType CardType { get; set; }

        public string Title { get; set; }

        [DataType(DataType.MultilineText)]
        public string Text { get; set; }

        [DataType(DataType.Url)]
        public string Url { get; set; }

        [CardFound]
        [CardOwned]
        public Guid? ParentCardID { get; set; }
    }
}