using System;
using ReadingListPlus.Common.Enums;
using ReadingListPlus.DataAccess.Models;

namespace ReadingListPlus.Web.Core.ViewModels
{
    public class CardViewModel
    {
        public Guid ID { get; set; }

        public Guid? DeckID { get; set; }

        public CardType Type { get; set; }

        public string DeckTitle { get; set; }

        public string Title { get; set; }

        public string Text { get; set; }

        public string HtmlText { get; set; }

        public string Url { get; set; }

        public int Position { get; set; }

        public bool IsBookmarked { get; set; }

        public string Selection { get; set; }

        public string NextAction { get; set; }

        public Priority? Priority { get; set; }
    }
}
