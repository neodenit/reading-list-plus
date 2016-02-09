using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadingListPlus
{
    public static class Scheduler
    {
        public static readonly Dictionary<Priority, Delays> Mapper = new Dictionary<Priority, Delays>
        {
            { Priority.High, Delays.Previous },
            { Priority.Medium, Delays.Same },
            { Priority.Low, Delays.Next },
        };

        private static readonly Random Random = new Random();

        public enum Delays
        {
            AfterNext,
            Next,
            Same,
            Previous,
            Initial,
        }

        public enum Priority
        {
            Low,
            Medium,
            High,
        }

        public static Priority ParsePriority(string text)
        {
            return (Priority)Enum.Parse(typeof(Priority), text, true);
        }

        public static void PromoteCard(IDeck deck, IEnumerable<ICard> cards, Delays delay)
        {
            var card = GetFirstCard(cards);

            var maxNewPosition = GetMaxNewPosition(cards);

            var step = GetStep(deck, delay, card.LastDelay);

            Debug.Assert(step > 0, "Step value is negative.");

            var newPosition = Math.Min(step, maxNewPosition);

            var newDelay = deck.AllowSmallDelays ? newPosition : Math.Max(newPosition, deck.StartDelay);

            ChangeFirstCardPosition(cards, deck.CorrectDelays, card, newPosition, newDelay);

            card.IsNew = false;
        }

        public static void AddCard(IDeck deck, ICollection<ICard> cards, ICard card)
        {
            PrepareForAdding(deck, cards, card);

            cards.Add(card);
        }

        public static void PrepareForAdding(IDeck deck, IEnumerable<ICard> cards, ICard card)
        {
            var maxNewPosition = GetMaxNewPosition(cards);

            card.Position = maxNewPosition;

            card.IsNew = true;
        }

        public static void PrepareForAdding(IDeck deck, IEnumerable<ICard> cards, ICard card, Priority priority)
        {
            Debug.Assert(deck.Type != DeckType.Spaced);

            var maxNewPosition = GetMaxNewPosition(cards);

            var position = deck.Type == DeckType.Predictable ?
                GetStaticPosition(priority, maxNewPosition) :
                GetRandomPosition(priority, maxNewPosition);

            PrepareForAdding(deck, cards, card, position);
        }

        public static void ShuffleNewCards(IEnumerable<ICard> cards)
        {
            var newCards = from item in cards where item.IsNew select item;

            ShuffleCards(newCards);
        }

        public static void ChangeFirstCardPosition(IDeck deck, IEnumerable<ICard> cards, ICard card, Priority priority)
        {
            Debug.Assert(deck.Type != DeckType.Spaced);

            var maxNewPosition = GetMaxNewPosition(cards);

            var position = deck.Type == DeckType.Predictable ?
                GetStaticPosition(priority, maxNewPosition) :
                GetRandomPosition(priority, maxNewPosition);

            ChangeFirstCardPosition(cards, deck.CorrectDelays, card, position);
        }

        /// <summary>
        /// Generates position in a range 1..max based on priority.
        /// </summary>
        /// <param name="priority">Priority of the card.</param>
        /// <param name="max">Max position to return.</param>
        /// <returns>Position of the card.</returns>
        public static int GetRandomPosition(Priority priority, int max, bool verbose = false)
        {
            Debug.Assert(max >= 1);

            if (max == 1)
            {
                return 1;
            }
            else if (max == 2)
            {
                switch (priority)
                {
                    case Priority.Low:
                        return 2;
                    case Priority.Medium:
                        return Random.Next(1, 2);
                    case Priority.High:
                        return 1;
                    default:
                        return -1;
                }
            }
            else
            {
                var split1 = 1.0 / 3.0;
                var split2 = 2.0 / 3.0;

                Func<double, int> op = Utils.Round;

                var hiMin = 1;
                var hiMax = op(max * split1);
                var medMin = hiMax + 1;
                var medMax = op(max * split2);
                var lowMin = medMax + 1;
                var lowMax = max;

                if (verbose)
                {
                    Console.WriteLine("{0} - {1}", hiMin, hiMax);
                    Console.WriteLine("{0} - {1}", medMin, medMax);
                    Console.WriteLine("{0} - {1}", lowMin, lowMax);
                    Console.WriteLine("---");

                    Console.WriteLine(hiMax - hiMin);
                    Console.WriteLine(medMax - medMin);
                    Console.WriteLine(lowMax - lowMin);
                    Console.WriteLine("---");
                }

                switch (priority)
                {
                    case Priority.Low:
                        return Random.Next(lowMin, lowMax);
                    case Priority.Medium:
                        return Random.Next(medMin, medMax);
                    case Priority.High:
                        return Random.Next(hiMin, hiMax);
                    default:
                        return -1;
                }
            }
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
                default:
                    return -1;
            }
        }

        private static void PrepareForAdding(IDeck deck, IEnumerable<ICard> cards, ICard card, int position)
        {
            ReservePosition(cards, position, deck.CorrectDelays);

            card.Position = position;

            card.IsNew = true;
        }

        private static void ShuffleCards(IEnumerable<ICard> cards)
        {
            var positions = from item in cards select item.Position;

            var shuffledNumbers = positions.OrderBy(item => Guid.NewGuid());

            var zip = Enumerable.Zip(cards, shuffledNumbers, (card, newPos) => new { card, newPos });

            zip.ToList().ForEach(item => item.card.Position = item.newPos);
        }

        private static void ChangeFirstCardPosition(IEnumerable<ICard> cards, bool correctDelays, ICard card, int newPosition)
        {
            PrepareFirstCardMove(cards, correctDelays, newPosition);

            card.Position = newPosition;
        }

        private static void ChangeFirstCardPosition(IEnumerable<ICard> cards, bool correctDelays, ICard card, int newPosition, int newDelay)
        {
            PrepareFirstCardMove(cards, correctDelays, newPosition);

            card.Position = newPosition;
            card.LastDelay = newDelay;
        }

        private static void PrepareFirstCardMove(IEnumerable<ICard> cards, bool correctDelays, int newPosition)
        {
            DecreasePositions(cards, newPosition);

            if (correctDelays)
            {
                CorrectDelays(cards, newPosition);
            }
        }

        private static void IncreasePositions(IEnumerable<ICard> cards, int position)
        {
            var more = from item in cards where item.Position >= position select item;

            foreach (var item in more)
            {
                item.Position++;
            }
        }

        private static void DecreasePositions(IEnumerable<ICard> cards, int position)
        {
            var less = from item in cards where item.Position <= position select item;

            foreach (var item in less)
            {
                item.Position--;
            }
        }

        private static void CorrectDelays(IEnumerable<ICard> cards, int newPosition)
        {
            var more = from item in cards where item.Position > newPosition && !item.IsNew select item;

            foreach (var item in more)
            {
                item.LastDelay++;
            }
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

        private static ICard GetFirstCard(IEnumerable<ICard> cards)
        {
            var card = cards.GetMinElement(item => item.Position);
            return card;
        }

        private static void ReservePosition(IEnumerable<ICard> cards, int position, bool correctDelays)
        {
            IncreasePositions(cards, position);

            if (correctDelays)
            {
                CorrectDelays(cards, position);
            }
        }

        private static int GetStep(IDeck deck, Delays delay, int lastDelay)
        {
            Func<double, int> op = Utils.Round;

            switch (delay)
            {
                case Delays.Initial:
                    return op(deck.StartDelay);
                case Delays.Previous:
                    return op(lastDelay / deck.Coeff);
                case Delays.Same:
                    return lastDelay;
                case Delays.Next:
                    return op(lastDelay * deck.Coeff);
                case Delays.AfterNext:
                    return op(lastDelay * deck.Coeff * deck.Coeff);
                default:
                    return -1;
            }
        }
    }
}
