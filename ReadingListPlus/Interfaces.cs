using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadingListPlus
{
    public enum DeckType
    {
        Predictable,
        Randomized,
        Spaced
    }

    public interface IDeck
    {
        int StartDelay { get; set; }

        double Coeff { get; set; }

        bool CorrectDelays { get; set; }

        bool AllowSmallDelays { get; set; }

        DeckType Type { get; set; }
    }

    public interface ICard
    {
        int Position { get; set; }

        int LastDelay { get; set; }

        bool IsNew { get; set; }
    }
}
