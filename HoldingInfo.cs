using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public struct HoldingInfo
    {
        public static HoldingInfo Empty = new HoldingInfo(-1, -1, -1, 0, 0);

        public int From { get; set; }
        public int FromIndex { get; set; }
        public int To { get; set; }
        public int Suits { get; set; }
        public int Length { get; set; }
        public int Next { get; set; }

        public HoldingInfo(int from, int fromIndex, int to, int suits, int length)
            : this()
        {
            From = from;
            To = to;
            FromIndex = fromIndex;
            Suits = suits;
            Length = length;
            Next = -1;
        }

        public override string ToString()
        {
            return string.Format("{0}/{1} -> {2}, Suits: {3}, Length: {4}, Next: {5}", From, FromIndex, To, Suits, Length, Next);
        }
    }
}
