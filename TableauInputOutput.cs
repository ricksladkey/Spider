using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public class TableauInputOutput
    {
        public static char Fence = '@';
        public static char PrimarySeparator = '|';
        public static char SecondarySeparator = '-';

        public Tableau Tableau { get; set; }

        public TableauInputOutput(Tableau tableau)
        {
            Tableau = tableau;
        }

        public TableauInputOutput(Game game)
        {
            Tableau = game.Tableau;
        }

        public string ToAsciiString()
        {
            return ToAsciiString(Tableau);
        }

        public static string ToAsciiString(Tableau tableau)
        {
            Pile discardRow = new Pile();
            for (int i = 0; i < tableau.DiscardPiles.Count; i++)
            {
                Pile discardPile = tableau.DiscardPiles[i];
                discardRow.Add(discardPile[discardPile.Count - 1]);
            }

            string s = "";

            s += Fence;
            s += tableau.Variation.ToAsciiString() + PrimarySeparator;
            s += ToAsciiString(discardRow) + PrimarySeparator;
            s += ToAsciiString(tableau.DownPiles) + PrimarySeparator;
            s += ToAsciiString(tableau.UpPiles) + PrimarySeparator;
            s += ToAsciiString(tableau.StockPile);
            s += Fence;

            return WrapString(s, 60);
        }

        private static string WrapString(string s, int columns)
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
            Variation variation = Variation.FromAsciiString(sections[0]);
            Pile discardCards = GetPileFromAsciiString(sections[1]);
            Pile[] downPiles = GetPilesFromAsciiString(sections[2]);
            Pile[] upPiles = GetPilesFromAsciiString(sections[3]);
            Pile stockPile = GetPileFromAsciiString(sections[4]);
            if (discardCards.Count > variation.NumberOfFoundations)
            {
                throw new Exception("too many discard piles");
            }
            if (downPiles.Length > variation.NumberOfPiles)
            {
                throw new Exception("wrong number of down piles");
            }
            if (upPiles.Length > variation.NumberOfPiles)
            {
                throw new Exception("wrong number of up piles");
            }
            if (stockPile.Count > variation.NumberOfStockCards)
            {
                throw new Exception("too many stock pile cards");
            }

            // Prepare game.
            Tableau.Variation = variation;
            Tableau.ClearAll();
            foreach (Card discardCard in discardCards)
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
                Tableau.DownPiles[column].Copy(downPiles[column]);
            }
            for (int column = 0; column < upPiles.Length; column++)
            {
                Tableau.UpPiles[column].Copy(upPiles[column]);
            }
            Tableau.StockPile.Copy(stockPile);
            Tableau.Refresh();
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

        public void FromTableau(Tableau tableau)
        {
            Tableau.Variation = tableau.Variation;
            Tableau.ClearAll();
            foreach (Pile pile in tableau.DiscardPiles)
            {
                Tableau.DiscardPiles.Add(pile);
            }
            for (int column = 0; column < Tableau.NumberOfPiles; column++)
            {
                Tableau.DownPiles[column].Copy((tableau.DownPiles[column]));
            }
            for (int column = 0; column < Tableau.NumberOfPiles; column++)
            {
                Tableau.UpPiles[column].Copy((tableau.UpPiles[column]));
            }
            Tableau.StockPile.Copy((tableau.StockPile));
            Tableau.Refresh();
        }

        public string ToPrettyString()
        {
            return ToPrettyString(Tableau);
        }

        public static string ToPrettyString(Tableau tableau)
        {
            string s = Environment.NewLine;
            s += "   Spider";
            s += Environment.NewLine;
            s += "--------------------------------";
            s += Environment.NewLine;
            Pile discardRow = new Pile();
            for (int i = 0; i < tableau.DiscardPiles.Count; i++)
            {
                Pile discardPile = tableau.DiscardPiles[i];
                discardRow.Add(discardPile[discardPile.Count - 1]);
            }
            s += ToPrettyString(-1, discardRow);
            s += Environment.NewLine;
            s += ToPrettyString(tableau.DownPiles);
            s += Environment.NewLine;
            s += "    " + ColumnHeadings(tableau.NumberOfPiles);
            s += Environment.NewLine;
            s += ToPrettyString(tableau.UpPiles);
            s += Environment.NewLine;
            int rowIndex = 0;
            Pile row = new Pile();
            for (int index = tableau.StockPile.Count - 1; index >= 0; index--)
            {
                row.Add(tableau.StockPile[index]);
                if (row.Count == tableau.NumberOfPiles)
                {
                    s += ToPrettyString(rowIndex++, row);
                    row.Clear();
                }
            }
            if (row.Count != 0)
            {
                s += ToPrettyString(rowIndex, row);
            }

            return s;
        }

        private static string ColumnHeadings(int columns)
        {
            string text = "";
            for (int column = 0; column < columns; column++)
            {
                text += string.Format("{0,-3}", column);
            }
            return text;
        }

        private static string ToPrettyString(IList<Pile> piles)
        {
            string s = "";
            int max = 0;
            for (int i = 0; i < piles.Count; i++)
            {
                max = Math.Max(max, piles[i].Count);
            }
            for (int j = 0; j < max; j++)
            {
                Pile row = new Pile();
                for (int i = 0; i < piles.Count; i++)
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

        public static void PrintGamesSideBySide(Tableau tableau1, Tableau tableau2)
        {
            string[] v1 = ToPrettyString(tableau1).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string[] v2 = ToPrettyString(tableau2).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
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
