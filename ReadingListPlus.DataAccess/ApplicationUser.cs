using System;
using Microsoft.AspNetCore.Identity;

namespace ReadingListPlus.DataAccess
{
    public class ApplicationUser : IdentityUser
    {
        public Guid? LastDeck { get; set; }
    }
}