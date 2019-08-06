using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReadingListPlus.DataAccess.Models;

namespace ReadingListPlus.Web.Core.ViewModels
{
    public class CreateCardViewModel : Card
    {
        [Required]
        public Priority? Priority { get; set; }

        public IEnumerable<SelectListItem> PriorityList { get; set; }
    }
}