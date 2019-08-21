using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReadingListPlus.Common.App_GlobalResources;
using ReadingListPlus.Common.Enums;
using ReadingListPlus.DataAccess.Models;

namespace ReadingListPlus.Web.Core.ViewModels
{
    public class CreateCardViewModel
    {
        [Required]
        public Priority? Priority { get; set; }

        public IEnumerable<SelectListItem> PriorityList { get; set; }

        [Required]
        [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ItemCollection))]
        public Guid? DeckID { get; set; }

        public IEnumerable<SelectListItem> DeckListItems { get; set; }

        public string DeckTitle { get; set; }

        public CardType Type { get; set; }

        public string Title { get; set; }

        [DataType(DataType.MultilineText)]
        public string Text { get; set; }

        [DataType(DataType.Url)]
        public string Url { get; set; }

        public Guid? ParentCardID { get; set; }

        public string ParentCardUpdatedText { get; set; }

        public Guid? OldDeckID { get; set; }
    }
}