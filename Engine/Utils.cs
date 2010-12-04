using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Spider.Collections;

namespace Spider.Engine
{
    public static class Utils
    {
        public static void Print(Tableau tableau)
        {
            Utils.ColorizeToConsole(TableauInputOutput.ToPrettyString(tableau));
        }

        public static void PrintSideBySide(Tableau tableau1, Tableau tableau2)
        {
            string[] v1 = TableauInputOutput.ToPrettyString(tableau1).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string[] v2 = TableauInputOutput.ToPrettyString(tableau2).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            int max = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                max = Math.Max(max, v1[i].Length);
            }
            string text = "";
            for (int i = 0; i < v1.Length || i < v2.Length; i++)
            {
                string s1 = i < v1.Length ? v1[i] : "";
                string s2 = i < v2.Length ? v2[i] : "";
                text += s1.PadRight(max + 1) + s2 + Environment.NewLine;
            }
            Utils.ColorizeToConsole(text);
        }

        private static object ConsoleMutex = new object();

        public static void ColorizeToConsole(string text)
        {
            lock (ConsoleMutex)
            {
                UnsafeColorizeToConsole(text);
            }
        }

        public static void UnsafeColorizeToConsole(string text)
        {
            // Extract unicode suit characters.
            char black1 = Suit.Spades.ToPrettyString()[0];
            char black2 = Suit.Clubs.ToPrettyString()[0];
            char red1 = Suit.Hearts.ToPrettyString()[0];
            char red2 = Suit.Diamonds.ToPrettyString()[0];
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
