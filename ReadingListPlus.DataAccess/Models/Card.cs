using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using ReadingListPlus.Common.Interfaces;

namespace ReadingListPlus.DataAccess.Models
{
    public enum CardType
    {
        Common = 0,
        Article = 1,
        Extract = 2,
    }

    public class Card : ICard
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid ID { get; set; }

        public Guid? DeckID { get; set; }

        [JsonIgnore]
        public Deck Deck { get; set; }

        public CardType Type { get; set; }

        public string Title { get; set; }

        [Required]
        public string Text { get; set; }

        public int Position { get; set; }

        public string Url { get; set; }

        [ForeignKey(nameof(Card))]
        public Guid? ParentCardID { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(ParentCardID))]
        public Card ParentCard { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(ParentCardID))]
        public ICollection<Card> ChildCards { get; set; }

        public bool IsAuthorized(string userName)
        {
            var result = Deck.OwnerID == userName;
            return result;
        }
    }
}