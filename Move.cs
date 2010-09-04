using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    public struct Move
    {
        public int From { get; set; }
        public int FromIndex { get; set; }
        public int To { get; set; }
        public int ToIndex { get; set; }
        public int HoldingNext { get; set; }
        public int OffloadPile { get; set; }
        public int OffloadIndex { get; set; }
        public int Next { get; set; }
        public double Score { get; set; }

        public Move(int from, int to)
            : this(from, -1, to, -1, -1, -1, -1, -1)
        {
        }

        public Move(int from, int fromIndex, int to)
            : this(from, fromIndex, to, -1, -1, -1, -1, -1)
        {
        }

        public Move(int from, int fromIndex, int to, int toIndex)
            : this(from, fromIndex, to, toIndex, -1, -1, -1, -1)
        {
        }

        public Move(int from, int fromIndex, int to, int toIndex, int holdingNext)
            : this(from, fromIndex, to, toIndex, holdingNext, -1, -1, -1)
        {
        }

        public Move(int from, int fromIndex, int to, int toIndex, int holdingNext, int offloadPile, int offloadIndex, int next)
            : this()
        {
            From = from;
            FromIndex = fromIndex;
            To = to;
            ToIndex = toIndex;
            HoldingNext = holdingNext;
            OffloadPile = offloadPile;
            OffloadIndex = offloadIndex;
            Next = next;
            Score = 0;
        }

        public override string ToString()
        {
            return string.Format("{0}/{1} -> {2}/{3} h{4} o{5}/{6}, n{7}: s{8}", From, FromIndex, To, ToIndex, HoldingNext, OffloadPile, OffloadIndex, Next, Score);
        }
    }
}
