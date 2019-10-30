using System;
using ReadingListPlus.Common.Enums;

namespace ReadingListPlus.DataAccess.Models
{
    public class ImportExportCard
    {
        public Guid ID { get; set; }

        public Guid? DeckID { get; set; }

        public CardType Type { get; set; }

        public string Title { get; set; }

        public string Text { get; set; }

        public int Position { get; set; }

        public string Url { get; set; }

        public Guid? ParentCardID { get; set; }
    }
}