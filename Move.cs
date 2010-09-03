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
        public int HoldingPile { get; set; }
        public int HoldingIndex { get; set; }
        public int OffloadPile { get; set; }
        public int OffloadIndex { get; set; }
        public int Next { get; set; }
        public double Score { get; set; }

        public Move(int from, int to)
            : this(from, -1, to, -1, -1, -1, -1, -1, -1)
        {
        }

        public Move(int from, int fromIndex, int to)
            : this(from, fromIndex, to, -1, -1, -1, -1, -1, -1)
        {
        }

        public Move(int from, int fromIndex, int to, int toIndex)
            : this(from, fromIndex, to, toIndex, -1, -1, -1, -1, -1)
        {
        }

        public Move(int from, int fromIndex, int to, int toIndex, int next)
            : this(from, fromIndex, to, toIndex, -1, -1, -1, -1, next)
        {
        }

        public Move(int from, int fromIndex, int to, int toIndex, int holdingPile, int holdingIndex)
            : this(from, fromIndex, to, toIndex, holdingPile, holdingIndex, -1, -1, -1)
        {
        }

        public Move(int from, int fromIndex, int to, int toIndex, int holdingPile, int holdingIndex, int offloadPile, int offloadIndex)
            : this(from, fromIndex, to, toIndex, holdingPile, holdingIndex, offloadPile, offloadIndex, -1)
        {
        }

        public Move(int from, int fromIndex, int to, int toIndex, int holdingPile, int holdingIndex, int offloadPile, int offloadIndex, int next)
            : this()
        {
            From = from;
            FromIndex = fromIndex;
            To = to;
            ToIndex = toIndex;
            HoldingPile = holdingPile;
            HoldingIndex = holdingPile != -1 ? holdingIndex : -1;
            OffloadPile = offloadPile;
            OffloadIndex = offloadIndex;
            Next = next;
            Score = 0;
        }

        public override string ToString()
        {
            return string.Format("{0}/{1} -> {2}/{3} h/ {4}/{5} o/ {6}/{7}, {8}: score {9}", From, FromIndex, To, ToIndex, HoldingPile, HoldingIndex, OffloadPile, OffloadIndex, Next, Score);
        }
    }
}
