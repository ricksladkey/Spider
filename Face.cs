using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public enum Face : byte
    {
        Empty = 0,
        Ace = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13,
    }

    public static class FaceExtensions
    {
        public static bool IsSourceFor(this Face face, Face other)
        {
            return face + 1 == other;
        }

        public static bool IsTargetFor(this Face face, Face other)
        {
            return face - 1 == other;
        }
    }
}
