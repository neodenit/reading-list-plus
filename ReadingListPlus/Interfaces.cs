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
    }

    public interface IDeck
    {
        DeckType Type { get; set; }
    }

    public interface ICard
    {
        int Position { get; set; }

        bool IsNew { get; set; }
    }
}
