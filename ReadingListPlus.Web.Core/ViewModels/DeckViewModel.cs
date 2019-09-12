using System;
using System.ComponentModel.DataAnnotations;

namespace ReadingListPlus.Web.Core.ViewModels
{
    public class DeckViewModel
    {
        public Guid ID { get; set; }

        [Required]
        public string Title { get; set; }

        public int CardCount { get; set; }
    }
}