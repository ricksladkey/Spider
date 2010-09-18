using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    public struct Move
    {
        public MoveType Type { get; set; }
        public MoveFlags Flags { get; set; }
        public int From { get; set; }
        public int FromIndex { get; set; }
        public int To { get; set; }
        public int ToIndex { get; set; }
        public int HoldingNext { get; set; }
        public int Next { get; set; }
        public double Score { get; set; }

        public Move(int from, int to)
            : this(MoveType.Basic, from, -1, to, -1, -1, -1)
        {
        }

        public Move(MoveType type, int from, int to)
            : this(type, from, -1, to, -1, -1, -1)
        {
        }

        public Move(int from, int fromIndex, int to)
            : this(MoveType.Basic, from, fromIndex, to, -1, -1, -1)
        {
        }

        public Move(MoveType type, int from, int fromIndex, int to)
            : this(type, from, fromIndex, to, -1, -1, -1)
        {
        }

        public Move(int from, int fromIndex, int to, int toIndex)
            : this(MoveType.Basic, from, fromIndex, to, toIndex, -1, -1)
        {
        }

        public Move(MoveType type, int from, int fromIndex, int to, int toIndex)
            : this(type, from, fromIndex, to, toIndex, -1, -1)
        {
        }

        public Move(int from, int fromIndex, int to, int toIndex, int holdingNext)
            : this(MoveType.Basic, from, fromIndex, to, toIndex, holdingNext, -1)
        {
        }

        public Move(MoveType type, int from, int fromIndex, int to, int toIndex, int holdingNext)
            : this(type, from, fromIndex, to, toIndex, holdingNext, -1)
        {
        }

        public Move(int from, int fromIndex, int to, int toIndex, int holdingNext, int next)
            : this(MoveType.Basic, from, fromIndex, to, toIndex, holdingNext, next)
        {
        }

        public Move(MoveType type, int from, int fromIndex, int to, int toIndex, int holdingNext, int next)
            : this(type, MoveFlags.Empty, from, fromIndex, to, toIndex, holdingNext, next)
        {
        }

        public Move(MoveType type, MoveFlags flags, int from, int fromIndex, int to, int toIndex, int holdingNext, int next)
            : this()
        {
            Type = type;
            Flags = flags;
            From = from;
            FromIndex = fromIndex;
            To = to;
            ToIndex = toIndex;
            HoldingNext = holdingNext;
            Next = next;
            Score = 0;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}/{2} -> {3}/{4} h{5}, n{6}: s{7}", Type, From, FromIndex, To, ToIndex, HoldingNext, Next, Score);
        }
    }
}
