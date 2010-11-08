using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Spider.Collections;

namespace Spider.Engine
{
    public struct HoldingInfo
    {
        public static HoldingInfo Empty = new HoldingInfo(-1, -1, -1, 0, 0);

        public HoldingInfo(int from, int fromRow, int to, int suits, int length)
            : this()
        {
            From = from;
            To = to;
            FromRow = fromRow;
            Suits = suits;
            Length = length;
            Next = -1;
        }

        public int From { get; set; }
        public int FromRow { get; set; }
        public int To { get; set; }
        public int Suits { get; set; }
        public int Length { get; set; }
        public int Next { get; set; }

        public override string ToString()
        {
            return string.Format("{0}/{1} -> {2}, Suits: {3}, Length: {4}, Next: {5}", From, FromRow, To, Suits, Length, Next);
        }
    }
}
