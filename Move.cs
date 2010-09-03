using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    public struct Move
    {
        public int From { get; private set; }
        public int FromIndex { get; private set; }
        public int To { get; private set; }
        public int ToIndex { get; private set; }
        public int HoldingPile { get; private set; }
        public int HoldingPileIndex { get; private set; }
        public int Next { get; set; }
        public double Score { get; set; }

        public Move(int from, int fromIndex, int to, int toIndex)
            : this(from, fromIndex, to, toIndex, -1, -1, -1)
        {
        }

        public Move(int from, int fromIndex, int to, int toIndex, int next)
            : this(from, fromIndex, to, toIndex, -1, -1, next)
        {
        }

        public Move(int from, int fromIndex, int to, int toIndex, int holdingPile, int holdingPileIndex)
            : this(from, fromIndex, to, toIndex, holdingPile, holdingPileIndex, -1)
        {
        }

        public Move(int from, int fromIndex, int to, int toIndex, int holdingPile, int holdingPileIndex, int next)
            : this()
        {
            From = from;
            FromIndex = fromIndex;
            To = to;
            ToIndex = toIndex;
            HoldingPile = holdingPile;
            HoldingPileIndex = holdingPile != -1 ? holdingPileIndex : -1;
            Next = next;
            Score = 0;
        }

        public override string ToString()
        {
            return string.Format("{0}/{1} -> {2}/{3} w/ {4}/{5}, {6}: {7}", From, FromIndex, To, ToIndex, HoldingPile, HoldingPileIndex, Next, Score);
        }
    }
}
