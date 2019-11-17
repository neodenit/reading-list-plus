using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using ReadingListPlus.Common;
using ReadingListPlus.Common.Enums;

namespace ReadingListPlus.DataAccess.Models
{
    public class Card
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid ID { get; set; }

        public Guid? DeckID { get; set; }

        [JsonIgnore]
        public Deck Deck { get; set; }

        [Required]
        public string OwnerID { get; set; }

        public CardType CardType { get; set; }

        public string Title { get; set; }

        [Required]
        public string Text { get; set; }

        public int Position { get; set; }

        [JsonIgnore]
        public bool IsConnected => Position != Constants.DisconnectedCardPosition;

        public string Url { get; set; }

        [ForeignKey(nameof(Card))]
        public Guid? ParentCardID { get; set; }

        [JsonIgnore]
        public Card ParentCard { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(ParentCardID))]
        public ICollection<Card> ChildCards { get; set; }
    }
}