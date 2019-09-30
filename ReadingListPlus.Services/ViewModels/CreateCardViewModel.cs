using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ReadingListPlus.Common.App_GlobalResources;
using ReadingListPlus.Common.Enums;
using ReadingListPlus.Services.Attributes;

namespace ReadingListPlus.Services.ViewModels
{
    public class CreateCardViewModel
    {
        [Required]
        public Priority? Priority { get; set; }

        public IEnumerable<KeyValuePair<string, string>> PriorityList { get; set; }

        [Required]
        [DeckFound]
        [DeckOwned]
        [Display(ResourceType = typeof(Resources), Name = nameof(Resources.Collection))]
        public Guid? DeckID { get; set; }

        public IEnumerable<DeckViewModel> DeckListItems { get; set; }

        public string DeckTitle { get; set; }

        public CardType Type { get; set; }

        public string Title { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }

        [DataType(DataType.Url)]
        public string Url { get; set; }

        [CardFound]
        [CardOwned]
        public Guid? ParentCardID { get; set; }

        public string ParentCardUpdatedText { get; set; }

        [DeckFound]
        [DeckOwned]
        public Guid? OldDeckID { get; set; }

        public CreationMode CreationMode { get; set; }
    }
}