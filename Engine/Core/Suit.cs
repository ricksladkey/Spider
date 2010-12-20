using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Spider.Collections;

namespace Spider.Engine
{
    public enum Suit : byte
    {
        Empty = 0 * 14,
        Spades = 1 * 14,
        Hearts = 2 * 14,
        Clubs = 3 * 14,
        Diamonds = 4 * 14,
    }

    public static class SuitExtensions
    {
        public static SuitColor GetColor(this Suit suit)
        {
            switch (suit)
            {
                case Suit.Spades:
                case Suit.Clubs:
                    return SuitColor.Black;
                case Suit.Hearts:
                case Suit.Diamonds:
                    return SuitColor.Red;
            }
            return SuitColor.Empty;
        }

        public static string ToAsciiString(this Suit suit)
        {
            switch (suit)
            {
                case Suit.Clubs:
                    return "c";
                case Suit.Diamonds:
                    return "d";
                case Suit.Hearts:
                    return "h";
                case Suit.Spades:
                    return "s";
            }
            return "-";
        }

        public static string ToPrettyString(this Suit suit)
        {
            switch (suit)
            {
                case Suit.Clubs:
                    return "♣";
                case Suit.Diamonds:
                    return "♦";
                case Suit.Hearts:
                    return "♥";
                case Suit.Spades:
                    return "♠";
            }
            return "-";
        }
    }

    public static class SuitUtils
    {
        public static Suit Parse(string s)
        {
            s = s.ToLowerInvariant();
            if (s == "c")
            {
                return Suit.Clubs;
            }
            if (s == "d")
            {
                return Suit.Diamonds;
            }
            if (s == "h")
            {
                return Suit.Hearts;
            }
            if (s == "s")
            {
                return Suit.Spades;
            }
            return Suit.Empty;
        }
    }
}
