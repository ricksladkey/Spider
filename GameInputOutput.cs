using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public class GameInputOutput : GameHelper
    {
        public static char Fence = '@';
        public static char PrimarySeparator = '|';
        public static char SecondarySeparator = '-';

        public GameInputOutput(Game game)
            : base(game)
        {
        }

        public string ToAsciiString()
        {
            Pile discardRow = new Pile();
            for (int i = 0; i < Tableau.DiscardPiles.Count; i++)
            {
                Pile discardPile = Tableau.DiscardPiles[i];
                discardRow.Add(discardPile[discardPile.Count - 1]);
            }

            string s = "";

            s += Fence;
            s += Suits.ToString() + PrimarySeparator;
            s += ToAsciiString(discardRow) + PrimarySeparator;
            s += ToAsciiString(Tableau.DownPiles) + PrimarySeparator;
            s += ToAsciiString(Tableau.UpPiles) + PrimarySeparator;
            s += ToAsciiString(Tableau.StockPile);
            s += Fence;

            return WrapString(s, 60);
        }

        private string WrapString(string s, int columns)
        {
            string t = "";
            while (s.Length > columns)
            {
                t += s.Substring(0, columns) + Environment.NewLine;
                s = s.Substring(columns);
            }
            return t + s;
        }

        private static string ToAsciiString(IList<Pile> piles)
        {
            string s = "";
            int n = piles.Count;
            while (n > 0 && piles[n - 1].Count == 0)
            {
                n--;
            }
            for (int i = 0; i < n; i++)
            {
                if (i != 0)
                {
                    s += SecondarySeparator;
                }
                s += ToAsciiString(piles[i]);
            }
            return s;
        }

        private static string ToAsciiString(Pile row)
        {
            string s = "";
            for (int i = 0; i < row.Count; i++)
            {
                s += row[i].ToAsciiString();
            }
            return s;
        }

        public void FromAsciiString(string s)
        {
            // Parse string.
            StringBuilder b = new StringBuilder();
            int i;
            for (i = 0; i < s.Length && s[i] != Fence; i++)
            {
            }
            if (i == s.Length)
            {
                throw new Exception("missing opening fence");
            }
            for (i++; i < s.Length && s[i] != Fence; i++)
            {
                char c = s[i];
                if (!char.IsWhiteSpace(c))
                {
                    b.Append(s[i]);
                }
            }
            if (i == s.Length)
            {
                throw new Exception("missing closing fence");
            }
            s = b.ToString();
            string[] sections = s.Split(PrimarySeparator);
            if (sections.Length != 5)
            {
                throw new Exception("wrong number of sections");
            }

            // Parse sections.
            int suits = int.Parse(sections[0]);
            if (suits != 1 && suits != 2 && suits != 4)
            {
                throw new Exception("invalid number of suits");
            }
            Pile discards = GetPileFromAsciiString(sections[1]);
            Pile[] downPiles = GetPilesFromAsciiString(sections[2]);
            Pile[] upPiles = GetPilesFromAsciiString(sections[3]);
            Pile stock = GetPileFromAsciiString(sections[4]);
            if (discards.Count > 8)
            {
                throw new Exception("too many discard piles");
            }
            if (downPiles.Length > NumberOfPiles)
            {
                throw new Exception("wrong number of down piles");
            }
            if (upPiles.Length > NumberOfPiles)
            {
                throw new Exception("wrong number of up piles");
            }
            if (stock.Count > 50)
            {
                throw new Exception("too many stock pile cards");
            }

            // Prepare game.
            Suits = suits;
            Initialize();
            foreach (Card discardCard in discards)
            {
                Pile discardPile = new Pile();
                for (Face face = Face.King; face >= Face.Ace; face--)
                {
                    discardPile.Add(new Card(face, discardCard.Suit));
                }
                Tableau.DiscardPiles.Add(discardPile);
            }
            for (int column = 0; column < downPiles.Length; column++)
            {
                Tableau.DownPiles[column] = downPiles[column];
            }
            for (int column = 0; column < upPiles.Length; column++)
            {
                Tableau.UpPiles[column] = upPiles[column];
            }
            Tableau.StockPile.AddRange(stock);
        }

        private static Pile[] GetPilesFromAsciiString(string s)
        {
            string[] rows = s.Split(SecondarySeparator);
            int n = rows.Length;
            Pile[] piles = new Pile[n];
            for (int i = 0; i < n; i++)
            {
                piles[i] = GetPileFromAsciiString(rows[i]);
            }
            return piles;
        }

        private static Pile GetPileFromAsciiString(string s)
        {
            int n = s.Length / 2;
            Pile pile = new Pile();
            for (int i = 0; i < n; i++)
            {
                pile.Add(Utils.GetCard(s.Substring(2 * i, 2)));
            }
            return pile;
        }

        public void FromGame(Game other)
        {
            Suits = other.Suits;
            Initialize();
            foreach (Pile pile in other.Tableau.DiscardPiles)
            {
                Tableau.DiscardPiles.Add(pile);
            }
            for (int column = 0; column < NumberOfPiles; column++)
            {
                Tableau.DownPiles[column].Copy((other.Tableau.DownPiles[column]));
            }
            for (int column = 0; column < NumberOfPiles; column++)
            {
                Tableau.UpPiles[column].Copy((other.Tableau.UpPiles[column]));
            }
            Tableau.StockPile.Copy((other.Tableau.StockPile));
        }

        public string ToPrettyString()
        {
            string s = Environment.NewLine;
            s += "   Spider";
            s += Environment.NewLine;
            s += "--------------------------------";
            s += Environment.NewLine;
            Pile discardRow = new Pile();
            for (int i = 0; i < Tableau.DiscardPiles.Count; i++)
            {
                Pile discardPile = Tableau.DiscardPiles[i];
                discardRow.Add(discardPile[discardPile.Count - 1]);
            }
            s += ToPrettyString(-1, discardRow);
            s += Environment.NewLine;
            s += ToPrettyString(Tableau.DownPiles);
            s += Environment.NewLine;
            s += "   0  1  2  3  4  5  6  7  8  9";
            s += Environment.NewLine;
            s += ToPrettyString(Tableau.UpPiles);
            s += Environment.NewLine;
            for (int i = 0; i < Tableau.StockPile.Count / NumberOfPiles; i++)
            {
                Pile row = new Pile();
                for (int j = 0; j < NumberOfPiles; j++)
                {
                    int index = i * NumberOfPiles + j;
                    int reverseIndex = Tableau.StockPile.Count - index - 1;
                    row.Add(Tableau.StockPile[reverseIndex]);
                }
                s += ToPrettyString(i, row);
            }

            return s;
        }

        private static string ToPrettyString(IList<Pile> piles)
        {
            string s = "";
            int max = 0;
            for (int i = 0; i < NumberOfPiles; i++)
            {
                max = Math.Max(max, piles[i].Count);
            }
            for (int j = 0; j < max; j++)
            {
                Pile row = new Pile();
                for (int i = 0; i < NumberOfPiles; i++)
                {
                    if (j < piles[i].Count)
                    {
                        row.Add(piles[i][j]);
                    }
                    else
                    {
                        row.Add(Card.Empty);
                    }
                }
                s += ToPrettyString(j, row);
            }
            return s;
        }

        private static string ToPrettyString(int row, Pile pile)
        {
            string s = "";
            if (row == -1)
            {
                s += "   ";
            }
            else
            {
                s += string.Format("{0,2} ", row);
            }
            for (int i = 0; i < pile.Count; i++)
            {
                if (i > 0)
                {
                    s += " ";
                }
                s += (pile[i].IsEmpty) ? "  " : pile[i].ToString();
            }
            return s + Environment.NewLine;
        }

        public static void PrintGamesSideBySide(Game game1, Game game2)
        {
            string[] v1 = game1.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string[] v2 = game2.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
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
    }
}
