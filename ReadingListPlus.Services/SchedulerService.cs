using System;
using System.Collections.Generic;
using System.Linq;
using ReadingListPlus.Common;
using ReadingListPlus.Common.Enums;

namespace ReadingListPlus.Services
{
    public class SchedulerService : ISchedulerService
    {
        public Priority ParsePriority(string text)
        {
            return (Priority)Enum.Parse(typeof(Priority), text, true);
        }

        public void PrepareForAdding(IDeck deck, IEnumerable<ICard> cards, ICard card, Priority priority)
        {
            var maxNewPosition = GetMaxNewPosition(cards);
            var position = GetStaticPosition(priority, maxNewPosition);

            PrepareForAdding(cards, card, position);
        }

        public void PrepareForDeletion(IEnumerable<ICard> cards, ICard card)
        {
            ExcludePosition(cards, card.Position);
        }

        public void ChangeFirstCardPosition(IDeck deck, IEnumerable<ICard> cards, ICard card, Priority priority)
        {
            var maxPosition = GetMaxPosition(cards);
            var position = GetStaticPosition(priority, maxPosition);

            ChangeFirstCardPosition(cards, card, position);
        }

        /// <summary>
        /// Returns position based on priority.
        /// </summary>
        /// <param name="priority">Priority of the card.</param>
        /// <param name="max">Max position to return.</param>
        /// <returns>Position of the card.</returns>
        private int GetStaticPosition(Priority priority, int max, bool verbose = false)
        {
            var split1 = 1.0 / 3.0;
            var split2 = 2.0 / 3.0;

            Func<double, int> op = Utils.Round;

            var hi = op(max * split1);
            var med = op(max * split2);
            var low = max;

            if (verbose)
            {
                Console.WriteLine("hi: {0}", hi);
                Console.WriteLine("med: {0}", med);
                Console.WriteLine("low: {0}", low);
                Console.WriteLine("---");
            }

            switch (priority)
            {
                case Priority.Low:
                    return low;
                case Priority.Medium:
                    return med;
                case Priority.High:
                    return hi;
                case Priority.Highest:
                    return Constants.FirstCardPosition;
                default:
                    return Constants.DisconnectedCardPosition;
            }
        }

        private void PrepareForAdding(IEnumerable<ICard> cards, ICard card, int position)
        {
            ReservePosition(cards, position);

            card.Position = position;
        }

        private void PrepareForDeletion(IEnumerable<ICard> cards, ICard card, int position)
        {
            ExcludePosition(cards, position);

            card.Position = Constants.DisconnectedCardPosition;
        }

        private void ShuffleCards(IEnumerable<ICard> cards)
        {
            var positions = from item in cards select item.Position;

            var shuffledNumbers = positions.OrderBy(item => Guid.NewGuid());

            var zip = Enumerable.Zip(cards, shuffledNumbers, (card, newPos) => new { card, newPos });

            zip.ToList().ForEach(item => item.card.Position = item.newPos);
        }

        private void ChangeFirstCardPosition(IEnumerable<ICard> cards, ICard card, int newPosition)
        {
            PrepareFirstCardMove(cards, newPosition);

            card.Position = newPosition;
        }

        private void PrepareFirstCardMove(IEnumerable<ICard> cards, int newPosition)
        {
            var highPriorityCards = GetHighPriorityCards(cards, newPosition);

            DecreasePositions(highPriorityCards, newPosition);
        }

        private void IncreasePositions(IEnumerable<ICard> cards, int position)
        {
            foreach (var item in cards)
            {
                item.Position++;
            }
        }

        private void DecreasePositions(IEnumerable<ICard> cards, int position)
        {
            foreach (var item in cards)
            {
                item.Position--;
            }
        }

        private IEnumerable<ICard> GetLowPriorityCards(IEnumerable<ICard> cards, int position)
        {
            return from item in cards where item.Position >= position select item;
        }

        private IEnumerable<ICard> GetLowPriorityCardsExclusive(IEnumerable<ICard> cards, int position)
        {
            return from item in cards where item.Position > position select item;
        }

        private IEnumerable<ICard> GetHighPriorityCards(IEnumerable<ICard> cards, int position)
        {
            return from item in cards where item.Position <= position select item;
        }

        private IEnumerable<ICard> GetHighPriorityCardsExclusive(IEnumerable<ICard> cards, int position)
        {
            return from item in cards where item.Position < position select item;
        }

        private int GetMaxNewPosition(IEnumerable<ICard> cards)
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

        private int GetMaxPosition(IEnumerable<ICard> cards)
        {
            if (cards.Any())
            {
                var max = cards.Max(item => item.Position);
                return max;
            }
            else
            {
                return Constants.FirstCardPosition;
            }
        }

        private ICard GetFirstCard(IEnumerable<ICard> cards)
        {
            var card = cards.GetMinElement(item => item.Position);
            return card;
        }

        private void ReservePosition(IEnumerable<ICard> cards, int position)
        {
            var lowPriorityCards = GetLowPriorityCards(cards, position);

            IncreasePositions(lowPriorityCards, position);
        }

        private void ExcludePosition(IEnumerable<ICard> cards, int position)
        {
            var lowPriorityCards = GetLowPriorityCardsExclusive(cards, position);

            DecreasePositions(lowPriorityCards, position);
        }
    }
}
