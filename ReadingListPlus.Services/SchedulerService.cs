using System;
using System.Collections.Generic;
using System.Linq;
using ReadingListPlus.Common;
using ReadingListPlus.Common.Enums;
using ReadingListPlus.DataAccess.Models;

namespace ReadingListPlus.Services
{
    public class SchedulerService : ISchedulerService
    {
        const double HighPriorityCoeff = 1.0 / 3.0;
        const double MediumPriorityCoeff = 2.0 / 3.0;

        public Priority ParsePriority(string text)
        {
            return (Priority)Enum.Parse(typeof(Priority), text, true);
        }

        public void PrepareForAdding(Deck deck, Card card, Priority priority)
        {
            var cards = deck.ConnectedCards;
            var maxNewPosition = GetMaxNewPosition(cards);
            var position = GetStaticPosition(priority, maxNewPosition);

            ReservePosition(cards, position);

            card.Position = position;
        }

        public void PrepareForDeletion(Deck deck, Card card)
        {
            ExcludePosition(deck.ConnectedCards, card.Position);
        }

        public void ChangeFirstCardPosition(Card card, Priority priority)
        {
            var cards = card.Deck.ConnectedCards;
            var maxPosition = GetMaxExistingPosition(cards);
            var newPosition = GetStaticPosition(priority, maxPosition);

            PrepareFirstCardMove(cards, newPosition);

            card.Position = newPosition;
        }

        public void ChangeCardPosition(Card card, Priority priority)
        {
            var allCards = card.Deck.ConnectedCards;
            var maxPosition = GetMaxExistingPosition(allCards);

            var oldPosition = card.Position;
            var newPosition = GetStaticPosition(priority, maxPosition);

            card.Position = Constants.DisconnectedCardPosition;

            var otherCards = card.Deck.ConnectedCards;

            PrepareCardMove(otherCards, oldPosition, newPosition);

            card.Position = newPosition;
        }

        public Card GetFirstCard(Deck deck)
        {
            var card = deck.ConnectedCards.Single(item => item.Position == Constants.FirstCardPosition);
            return card;
        }

        /// <summary>
        /// Returns position based on priority.
        /// </summary>
        /// <param name="priority">Priority of the card.</param>
        /// <param name="max">Max position to return.</param>
        /// <returns>Position of the card.</returns>
        private int GetStaticPosition(Priority priority, int max)
        {
            if (max <= 1)
            {
                return max;
            }
            else
            {
                switch (priority)
                {
                    case Priority.Low:
                        return max;
                    case Priority.Medium:
                        return Utils.Round(max * MediumPriorityCoeff);
                    case Priority.High:
                        return Utils.Round(max * HighPriorityCoeff);
                    case Priority.Highest:
                        return Constants.FirstCardPosition;
                    default:
                        return Constants.DisconnectedCardPosition;
                }
            }
        }

        private void ShuffleCards(IEnumerable<Card> cards)
        {
            var positions = from item in cards select item.Position;

            var shuffledNumbers = positions.OrderBy(item => Guid.NewGuid());

            var zip = Enumerable.Zip(cards, shuffledNumbers, (card, newPos) => new { card, newPos });

            zip.ToList().ForEach(item => item.card.Position = item.newPos);
        }

        private void PrepareFirstCardMove(IEnumerable<Card> otherCards, int newPosition)
        {
            var highPriorityCards = GetHighPriorityCards(otherCards, newPosition);

            DecreasePositions(highPriorityCards);
        }

        private void PrepareCardMove(IEnumerable<Card> otherCards, int oldPosition, int newPosition)
        {
            ExcludePosition(otherCards, oldPosition);
            ReservePosition(otherCards, newPosition);
        }

        private void IncreasePositions(IEnumerable<Card> cards)
        {
            foreach (var item in cards)
            {
                item.Position++;
            }
        }

        private void DecreasePositions(IEnumerable<Card> cards)
        {
            foreach (var item in cards)
            {
                item.Position--;
            }
        }

        private IEnumerable<Card> GetLowPriorityCards(IEnumerable<Card> cards, int position)
        {
            return from item in cards where item.Position >= position select item;
        }

        private IEnumerable<Card> GetLowPriorityCardsExclusive(IEnumerable<Card> cards, int position)
        {
            return from item in cards where item.Position > position select item;
        }

        private IEnumerable<Card> GetHighPriorityCards(IEnumerable<Card> cards, int position)
        {
            return from item in cards where item.Position <= position select item;
        }

        private IEnumerable<Card> GetHighPriorityCardsExclusive(IEnumerable<Card> cards, int position)
        {
            return from item in cards where item.Position < position select item;
        }

        private int GetMaxNewPosition(IEnumerable<Card> cards)
        {
            if (cards.Any())
            {
                var max = cards.Max(item => item.Position);
                var nextToMax = max + 1;
                return nextToMax;
            }
            else
            {
                return Constants.FirstCardPosition;
            }
        }

        private int GetMaxExistingPosition(IEnumerable<Card> allCards)
        {
            if (allCards.Any())
            {
                var max = allCards.Max(item => item.Position);
                return max;
            }
            else
            {
                return Constants.FirstCardPosition;
            }
        }

        private void ReservePosition(IEnumerable<Card> otherCards, int position)
        {
            var lowPriorityCards = GetLowPriorityCards(otherCards, position);

            IncreasePositions(lowPriorityCards);
        }

        private void ExcludePosition(IEnumerable<Card> otherCards, int position)
        {
            var lowPriorityCards = GetLowPriorityCardsExclusive(otherCards, position);

            DecreasePositions(lowPriorityCards);
        }
    }
}
