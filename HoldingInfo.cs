using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public struct HoldingInfo
    {
        public static HoldingInfo Empty = new HoldingInfo(-1, -1, 0, 0);

        public int Pile { get; set; }
        public int Index { get; set; }
        public int Suits { get; set; }
        public int Length { get; set; }
        public int Next { get; set; }

        public HoldingInfo(int pile, int index, int suits, int length)
            : this()
        {
            Pile = pile;
            Index = index;
            Suits = suits;
            Length = length;
            Next = -1;
        }

        public override string ToString()
        {
            return string.Format("Pile: {0}, Index: {1}, Suits: {2}, Length: {3}, Next: {4}", Pile, Index, Suits, Length, Next);
        }
    }
}
