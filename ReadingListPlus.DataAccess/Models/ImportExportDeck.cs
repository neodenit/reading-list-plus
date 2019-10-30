using System;
using System.Collections.Generic;

namespace ReadingListPlus.DataAccess.Models
{
    public class ImportExportDeck
    {
        public Guid ID { get; set; }

        public Guid? DependentDeckID { get; set; }

        public string Title { get; set; }

        public string OwnerID { get; set; }

        public ICollection<ImportExportCard> Cards { get; set; }
    }
}