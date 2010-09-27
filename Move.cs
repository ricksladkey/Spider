using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    public struct Move
    {
        public static Move Empty = new Move(-1, -1);

        public MoveType Type { get; set; }
        public MoveFlags Flags { get; set; }
        public int From { get; set; }
        public int FromRow { get; set; }
        public int To { get; set; }
        public int ToRow { get; set; }
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

        public Move(int from, int fromRow, int to)
            : this(MoveType.Basic, from, fromRow, to, -1, -1, -1)
        {
        }

        public Move(MoveType type, int from, int fromRow, int to)
            : this(type, from, fromRow, to, -1, -1, -1)
        {
        }

        public Move(MoveType type, MoveFlags flags, int from, int fromRow, int to)
            : this(type, flags, from, fromRow, to, -1, -1, -1)
        {
        }

        public Move(int from, int fromRow, int to, int toRow)
            : this(MoveType.Basic, from, fromRow, to, toRow, -1, -1)
        {
        }

        public Move(MoveType type, int from, int fromRow, int to, int toRow)
            : this(type, from, fromRow, to, toRow, -1, -1)
        {
        }

        public Move(MoveType type, MoveFlags flags, int from, int fromRow, int to, int toRow)
            : this(type, flags, from, fromRow, to, toRow, -1, -1)
        {
        }

        public Move(int from, int fromRow, int to, int toRow, int holdingNext)
            : this(MoveType.Basic, from, fromRow, to, toRow, holdingNext, -1)
        {
        }

        public Move(MoveType type, int from, int fromRow, int to, int toRow, int holdingNext)
            : this(type, from, fromRow, to, toRow, holdingNext, -1)
        {
        }

        public Move(int from, int fromRow, int to, int toRow, int holdingNext, int next)
            : this(MoveType.Basic, from, fromRow, to, toRow, holdingNext, next)
        {
        }

        public Move(MoveType type, int from, int fromRow, int to, int toRow, int holdingNext, int next)
            : this(type, MoveFlags.Empty, from, fromRow, to, toRow, holdingNext, next)
        {
        }

        public Move(MoveType type, MoveFlags flags, int from, int fromRow, int to, int toRow, int holdingNext, int next)
            : this()
        {
            Type = type;
            Flags = flags;
            From = from;
            FromRow = fromRow;
            To = to;
            ToRow = toRow;
            HoldingNext = holdingNext;
            Next = next;
            Score = 0;
        }

        public bool IsEmpty
        {
            get
            {
                return From == -1;
            }
        }

        public override string ToString()
        {
            string type = Type.ToString();
            if (Flags != MoveFlags.Empty)
            {
                type += "(" + Flags.ToString() + ")";
            }
            string score = Score.ToString("G5");
            if (Score == Game.RejectScore)
            {
                score = "Reject";
            }
            return string.Format("{0}: {1}/{2} -> {3}/{4} h{5}, n{6}: s{7}", type, From, FromRow, To, ToRow, HoldingNext, Next, score);
        }
    }
}
