using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace ReadingListPlus.Persistence.Models
{
    public class Deck : IDeck
    {
        public int ID { get; set; }

        [Required]
        public string Title { get; set; }

        public string OwnerID { get; set; }

        public virtual ICollection<Card> Cards { get; set; }

        public int StartDelay { get; set; }

        public double Coeff { get; set; }

        public bool AllowSmallDelays { get; set; }

        public bool CorrectDelays { get; set; }

        public bool IsAuthorized(IPrincipal user)
        {
            var userName = user.Identity.Name;

            return OwnerID == userName;
        }
    }
}