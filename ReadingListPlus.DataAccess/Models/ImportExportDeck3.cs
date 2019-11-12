using System;
using System.Collections.Generic;

namespace ReadingListPlus.DataAccess.Models
{
    public class ImportExportDeck3 : IImportExportDeck
    {
        public Guid ID { get; set; }

        public Guid? DependentDeckID { get; set; }

        public string Title { get; set; }

        public string OwnerID { get; set; }

        public ICollection<ImportExportCard3> Cards { get; set; }
    }
}