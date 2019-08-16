using System;
using System.ComponentModel.DataAnnotations;
using ReadingListPlus.DataAccess.Models;

namespace ReadingListPlus.Web.Core.ViewModels
{
    public class EditCardViewModel
    {
        public Guid ID { get; set; }

        public Guid? DeckID { get; set; }

        public string DeckTitle { get; set; }

        public CardType Type { get; set; }

        public string Title { get; set; }

        [DataType(DataType.MultilineText)]
        public string Text { get; set; }

        [DataType(DataType.Url)]
        public string Url { get; set; }

        public Guid? ParentCardID { get; set; }
    }
}