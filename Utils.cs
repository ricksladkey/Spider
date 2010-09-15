using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    public static class Utils
    {
        private static object ConsoleMutex { get; set; }

        public static string GetString(Face face)
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

        public static string GetPrettyString(Suit suit)
        {
            switch (suit)
            {
                case Suit.Clubs:
                    return "♠";
                case Suit.Diamonds:
                    return "♦";
                case Suit.Hearts:
                    return "♥";
                case Suit.Spades:
                    return "♠";
            }
            return "-";
        }

        public static string GetAsciiString(Suit suit)
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

        public static Face GetFace(string s)
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

        public static Suit GetSuit(string s)
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

        public static Card GetCard(string s)
        {
            return new Card(GetFace(s.Substring(0, 1)), GetSuit(s.Substring(1, 1)));
        }

        public static void ColorizeToConsole(string text)
        {
            lock (ConsoleMutex)
            {
                UnsafeColorizeToConsole(text);
            }
        }

        public static void UnsafeColorizeToConsole(string text)
        {
            char black1 = Utils.GetPrettyString(Suit.Spades)[0];
            char black2 = Utils.GetPrettyString(Suit.Clubs)[0];
            char red1 = Utils.GetPrettyString(Suit.Hearts)[0];
            char red2 = Utils.GetPrettyString(Suit.Diamonds)[0];
            char[] suits = new char[] { black1, black2, red1, red2, };
            int position = 0;
            while (position < text.Length)
            {
                int next = text.IndexOfAny(suits, position);
                if (next == -1)
                {
                    Console.Write(text.Substring(position));
                    position = text.Length;
                    continue;
                }
                char nextChar = text[next];
                ConsoleColor color =
                    nextChar == black1 || nextChar == black2 ?
                    ConsoleColor.Black :
                    ConsoleColor.DarkRed;
                Console.Write(text.Substring(position, next - position - 1));
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = color;
                Console.Write(text.Substring(next - 1, 2));
                Console.ResetColor();
                position = next + 1;
            }
            if (Debugger.IsAttached)
            {
                Trace.WriteLine(text);
            }
        }

        public static void WriteLine(object obj)
        {
            WriteLine("{0}", obj);
        }

        public static void WriteLine(string format, params object[] args)
        {
            Console.WriteLine(format, args);
            if (Debugger.IsAttached)
            {
                Trace.WriteLine(string.Format(format, args));
            }
        }
    }
}
