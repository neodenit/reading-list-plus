using ReadingListPlus.Persistence.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace ReadingListPlus.Web.Models
{
    public class CreateCardViewModel : Card
    {
        [Required]
        public Priority? Priority { get; set; }

        public IEnumerable<SelectListItem> PriorityList { get; set; }
    }
}