using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;

namespace ReadingListPlus.Persistence.Models
{
    public class Deck : IDeck
    {
        public int ID { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string OwnerID { get; set; }

        public virtual ICollection<Card> Cards { get; set; }

        public IEnumerable<Card> ConnectedCards => Cards.Where(c => c.Position != Constants.DisconnectedCardPosition);

        public bool IsAuthorized(IPrincipal user)
        {
            var userName = user.Identity.Name;

            return OwnerID == userName;
        }
    }
}