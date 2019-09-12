using System;
using System.ComponentModel.DataAnnotations;
using ReadingListPlus.Web.Core.Attributes;

namespace ReadingListPlus.Web.Core.ViewModels
{
    public class DeckViewModel
    {
        [DeckFound]
        [DeckOwned]
        public Guid ID { get; set; }

        [Required]
        public string Title { get; set; }

        public int CardCount { get; set; }
    }
}