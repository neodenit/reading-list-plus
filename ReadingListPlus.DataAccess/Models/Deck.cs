using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Principal;

namespace ReadingListPlus.DataAccess.Models
{
    public class Deck : IDeck
    {
        public int ID { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string OwnerID { get; set; }

        public ICollection<Card> Cards { get; set; }

        [NotMapped]
        public IEnumerable<Card> ConnectedCards => Cards.Where(c => c.Position != Constants.DisconnectedCardPosition);

        public bool IsAuthorized(IPrincipal user)
        {
            var userName = user.Identity.Name;

            return OwnerID == userName;
        }
    }
}