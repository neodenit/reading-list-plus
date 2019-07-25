using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ReadingListPlus
{
    public enum Priority
    {
        [Display(Name = "Highest (not recommended)")]
        Highest,
        High,
        Medium,
        Low,
    }

    public static class Scheduler
    {
        private static readonly Random Random = new Random();

        public static Priority ParsePriority(string text)
        {
            return (Priority)Enum.Parse(typeof(Priority), text, true);
        }

        public static void PrepareForAdding(IDeck deck, IEnumerable<ICard> cards, ICard card, Priority priority)
        {
            var maxNewPosition = GetMaxNewPosition(cards);
            var position = GetStaticPosition(priority, maxNewPosition);

            PrepareForAdding(cards, card, position);
        }

        public static void PrepareForDeletion(IEnumerable<ICard> cards, ICard card)
        {
            ExcludePosition(cards, card.Position);
        }

        public static void ChangeFirstCardPosition(IDeck deck, IEnumerable<ICard> cards, ICard card, Priority priority)
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
        public static int GetStaticPosition(Priority priority, int max, bool verbose = false)
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
                    return 0;
                default:
                    return -1;
            }
        }

        private static void PrepareForAdding(IEnumerable<ICard> cards, ICard card, int position)
        {
            ReservePosition(cards, position);

            card.Position = position;
        }

        private static void PrepareForDeletion(IEnumerable<ICard> cards, ICard card, int position)
        {
            ExcludePosition(cards, position);

            card.Position = -1;
        }

        private static void ShuffleCards(IEnumerable<ICard> cards)
        {
            var positions = from item in cards select item.Position;

            var shuffledNumbers = positions.OrderBy(item => Guid.NewGuid());

            var zip = Enumerable.Zip(cards, shuffledNumbers, (card, newPos) => new { card, newPos });

            zip.ToList().ForEach(item => item.card.Position = item.newPos);
        }

        private static void ChangeFirstCardPosition(IEnumerable<ICard> cards, ICard card, int newPosition)
        {
            PrepareFirstCardMove(cards, newPosition);

            card.Position = newPosition;
        }

        private static void PrepareFirstCardMove(IEnumerable<ICard> cards, int newPosition)
        {
            var highPriorityCards = GetHighPriorityCards(cards, newPosition);

            DecreasePositions(highPriorityCards, newPosition);
        }

        private static void IncreasePositions(IEnumerable<ICard> cards, int position)
        {
            foreach (var item in cards)
            {
                item.Position++;
            }
        }

        private static void DecreasePositions(IEnumerable<ICard> cards, int position)
        {
            foreach (var item in cards)
            {
                item.Position--;
            }
        }

        private static IEnumerable<ICard> GetLowPriorityCards(IEnumerable<ICard> cards, int position)
        {
            return from item in cards where item.Position >= position select item;
        }

        private static IEnumerable<ICard> GetLowPriorityCardsExclusive(IEnumerable<ICard> cards, int position)
        {
            return from item in cards where item.Position > position select item;
        }

        private static IEnumerable<ICard> GetHighPriorityCards(IEnumerable<ICard> cards, int position)
        {
            return from item in cards where item.Position <= position select item;
        }

        private static IEnumerable<ICard> GetHighPriorityCardsExclusive(IEnumerable<ICard> cards, int position)
        {
            return from item in cards where item.Position < position select item;
        }

        private static int GetMaxNewPosition(IEnumerable<ICard> cards)
        {
            if (cards.Any())
            {
                var max = cards.Max(item => item.Position);
                var nextToMax = max + 1;
                return nextToMax;
            }
            else
            {
                return 0;
            }
        }

        private static int GetMaxPosition(IEnumerable<ICard> cards)
        {
            if (cards.Any())
            {
                var max = cards.Max(item => item.Position);
                return max;
            }
            else
            {
                return 0;
            }
        }

        private static ICard GetFirstCard(IEnumerable<ICard> cards)
        {
            var card = cards.GetMinElement(item => item.Position);
            return card;
        }

        private static void ReservePosition(IEnumerable<ICard> cards, int position)
        {
            var lowPriorityCards = GetLowPriorityCards(cards, position);

            IncreasePositions(lowPriorityCards, position);
        }

        private static void ExcludePosition(IEnumerable<ICard> cards, int position)
        {
            var lowPriorityCards = GetLowPriorityCardsExclusive(cards, position);

            DecreasePositions(lowPriorityCards, position);
        }
    }
}
