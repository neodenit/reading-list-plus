﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Principal;
using Newtonsoft.Json;
using ReadingListPlus.Common;
using ReadingListPlus.Common.Interfaces;

namespace ReadingListPlus.DataAccess.Models
{
    public class Deck : IDeck
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid ID { get; set; }

        [ForeignKey(nameof(Deck))]
        public Guid? DependentDeckID { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(DependentDeckID))]
        public Deck DependentDeck { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string OwnerID { get; set; }

        public ICollection<Card> Cards { get; set; }

        [JsonIgnore]
        [NotMapped]
        public IEnumerable<Card> ConnectedCards => Cards?.Where(c => c.Position != Constants.DisconnectedCardPosition);

        public bool IsAuthorized(IPrincipal user)
        {
            var userName = user.Identity.Name;

            return OwnerID == userName;
        }
    }
}