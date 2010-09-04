using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public struct HoldingInfo
    {
        public int Pile { get; set; }
        public int Index { get; set; }
        public int Suits { get; set; }
        public int Next { get; set; }

        public HoldingInfo(int pile, int index, int suits)
            : this()
        {
            Pile = pile;
            Index = index;
            Suits = suits;
            Next = -1;
        }

        public override string ToString()
        {
            return string.Format("Pile: {0}, Index: {1}, Suits: {2}, Next: {3}", Pile, Index, Suits, Next);
        }
    }
}
