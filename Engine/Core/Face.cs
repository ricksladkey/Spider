using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Spider.Engine.Collections;

namespace Spider.Engine.Core
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
            return face + 1 == other && face != Face.Empty;
        }

        public static bool IsTargetFor(this Face face, Face other)
        {
            return face - 1 == other && other != Face.Empty;
        }

        public static string ToAsciiString(this Face face)
        {
            switch (face)
            {
                case Face.Ace:
                    return "A";
                case Face.Two:
                    return "2";
                case Face.Three:
                    return "3";
                case Face.Four:
                    return "4";
                case Face.Five:
                    return "5";
                case Face.Six:
                    return "6";
                case Face.Seven:
                    return "7";
                case Face.Eight:
                    return "8";
                case Face.Nine:
                    return "9";
                case Face.Ten:
                    return "T";
                case Face.Jack:
                    return "J";
                case Face.Queen:
                    return "Q";
                case Face.King:
                    return "K";
            }
            return "-";
        }

        public static string ToLabel(this Face face)
        {
            if (face == Face.Ten)
            {
                return "10";
            }
            return ToAsciiString(face);
        }

        public static string ToPrettyString(this Face face)
        {
            return ToAsciiString(face);
        }
    }

    public static class FaceUtils
    {
        public static Face Parse(string s)
        {
            s = s.ToUpperInvariant();
            if (s == "A")
            {
                return Face.Ace;
            }
            if (s == "2")
            {
                return Face.Two;
            }
            if (s == "3")
            {
                return Face.Three;
            }
            if (s == "4")
            {
                return Face.Four;
            }
            if (s == "5")
            {
                return Face.Five;
            }
            if (s == "6")
            {
                return Face.Six;
            }
            if (s == "7")
            {
                return Face.Seven;
            }
            if (s == "8")
            {
                return Face.Eight;
            }
            if (s == "9")
            {
                return Face.Nine;
            }
            if (s == "T")
            {
                return Face.Ten;
            }
            if (s == "J")
            {
                return Face.Jack;
            }
            if (s == "Q")
            {
                return Face.Queen;
            }
            if (s == "K")
            {
                return Face.King;
            }
            return Face.Empty;
        }
    }
}
