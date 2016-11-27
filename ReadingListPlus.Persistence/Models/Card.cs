using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace ReadingListPlus.Persistence.Models
{
    public class Card : ICard
    {
        public int ID { get; set; }

        public int DeckID { get; set; }

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

        public string Discriminator { get; set; }

        public bool IsAuthorized(IPrincipal user)
        {
            var userName = user.Identity.Name;

            return Deck.OwnerID == userName;
        }
    }
}