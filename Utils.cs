using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    static class Utils
    {
        public static string ToString(Face face)
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

        public static string ToString(Suit suit)
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

        public static void ColorizeToConsole(string text)
        {
            char black1 = Utils.ToString(Suit.Spades)[0];
            char black2 = Utils.ToString(Suit.Clubs)[0];
            char red1 = Utils.ToString(Suit.Hearts)[0];
            char red2 = Utils.ToString(Suit.Diamonds)[0];
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
        }
    }
}
