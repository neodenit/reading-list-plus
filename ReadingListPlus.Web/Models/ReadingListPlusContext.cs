using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Principal;
using System.Web;
using Microsoft.AspNet.Identity;

namespace ReadingListPlus.Web.Models
{
    public class ReadingListPlusContext : DbContext
    {
        public ReadingListPlusContext() : base("DefaultConnection") { }

        public System.Data.Entity.DbSet<ReadingListPlus.Web.Models.Deck> Decks { get; set; }

        public System.Data.Entity.DbSet<ReadingListPlus.Web.Models.Card> Cards { get; set; }

        public IQueryable<Deck> GetUserDecks(IPrincipal user)
        {
            var userID = user.Identity.GetUserId();
            var items = Decks.Where(item => item.OwnerID == userID);

            return items;
        }
    }
}