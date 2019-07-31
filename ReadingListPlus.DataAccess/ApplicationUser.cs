using Microsoft.AspNetCore.Identity;

namespace ReadingListPlus.DataAccess
{
    public class ApplicationUser : IdentityUser
    {
        public int? LastDeck { get; set; }
    }
}