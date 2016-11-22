using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Script.Serialization;
using Microsoft.AspNet.Identity;
using System.Web.Mvc;

namespace ReadingListPlus.Web.Models
{
    public class Deck : IDeck
    {
        public int ID { get; set; }

        [Required]
        public string Title { get; set; }

        public string OwnerID { get; set; }

        public virtual ICollection<Card> Cards { get; set; }

        public DeckType Type { get; set; }

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

    public class Card : ICard
    {
        public int ID { get; set; }

        public int DeckID { get; set; }

        [ScriptIgnore(ApplyToOverrides = true)]
        public virtual Deck Deck { get; set; }

        public string Title { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }

        public int Position { get; set; }

        [DataType(DataType.Url)]
        public string Url { get; set; }

        public bool IsNew { get; set; }

        public string Selection { get; set; }

        public string NextAction { get; set; }

        public int ParentCardID { get; set; }

        public int LastDelay { get; set; }

        public double Coeff { get; set; }

        public bool IsAuthorized(IPrincipal user)
        {
            var userName = user.Identity.Name;

            return Deck.OwnerID == userName;
        }
    }

    public class CreateCardViewModel : Card
    {
        [Required]
        public Priority? Priority { get; set; }

        public IEnumerable<SelectListItem> PriorityList { get; set; }
    }
}