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
            var userName = user.Identity.Name;
            var items = Decks.Where(item => item.OwnerID == userName);

            return items;
        }
    }
}