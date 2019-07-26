using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;
using System.Web.Script.Serialization;

namespace ReadingListPlus.Persistence.Models
{
    public enum CardType
    {
        Common = 0,
        Article = 1,
        Extract = 2,
    }

    public class Card : ICard
    {
        public int ID { get; set; }

        public int DeckID { get; set; }

        [ScriptIgnore(ApplyToOverrides = true)]
        public virtual Deck Deck { get; set; }

        public CardType Type { get; set; }

        public string Title { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }

        [NotMapped]
        public string HtmlText { get; set; }

        public int Position { get; set; }

        [DataType(DataType.Url)]
        public string Url { get; set; }

        [NotMapped]
        public bool IsBookmarked { get; set; }

        [NotMapped]
        public string Selection { get; set; }

        [NotMapped]
        public string NextAction { get; set; }

        public int? ParentCardID { get; set; }

        [ScriptIgnore(ApplyToOverrides = true)]
        public virtual Card ParentCard { get; set; }

        [ScriptIgnore(ApplyToOverrides = true)]
        public virtual ICollection<Card> ChildCards { get; set; }

        public bool IsAuthorized(IPrincipal user)
        {
            var userName = user.Identity.Name;

            return Deck.OwnerID == userName;
        }
    }
}