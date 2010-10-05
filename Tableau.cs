using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    [DebuggerDisplay("Count = {count}")]
    [DebuggerTypeProxy(typeof(EnumerableDebugView))]
    public class Tableau : IGetCard
    {
        private bool autoAdjust;
        private int count;
        private Pile[] downPiles;
        private Pile[] upPiles;
        private FastList<Pile> discardPiles;
        private Pile scratchPile;

        public Tableau()
            : this(true)
        {
        }

        public Tableau(bool autoAdjust)
        {
            this.autoAdjust = autoAdjust;
            count = Game.NumberOfPiles;
            downPiles = new Pile[count];
            upPiles = new Pile[count];
            discardPiles = new FastList<Pile>(count);
            for (int row = 0; row < count; row++)
            {
                downPiles[row] = new Pile();
                upPiles[row] = new Pile();
            }
            scratchPile = new Pile();
        }

        public Pile this[int index]
        {
            get
            {
                return upPiles[index];
            }
        }

        public IList<Pile> DownPiles
        {
            get
            {
                return downPiles;
            }
        }

        public IList<Pile> UpPiles
        {
            get
            {
                return upPiles;
            }
        }

        public IList<Pile> DiscardPiles
        {
            get
            {
                return discardPiles;
            }
        }

        public void ClearAll()
        {
            for (int i = 0; i < count; i++)
            {
                downPiles[i].Clear();
                upPiles[i].Clear();
            }
            discardPiles.Clear();
        }

        public int GetDownCount(int column)
        {
            return downPiles[column].Count;
        }

        public int GetRunDown(int column, int row)
        {
            return upPiles[column].GetRunDown(row);
        }

        public int GetRunDownAnySuit(int column, int row)
        {
            return upPiles[column].GetRunDownAnySuit(row);
        }

        public int GetRunUp(int column, int row)
        {
            return upPiles[column].GetRunUp(row);
        }

        public int GetRunUpAnySuit(int column, int row)
        {
            return upPiles[column].GetRunUpAnySuit(row);
        }

        public int CountSuits(int column, int row)
        {
            return upPiles[column].CountSuits(row, -1);
        }

        public int CountSuits(int column, int startRow, int endRow)
        {
            return upPiles[column].CountSuits(startRow, endRow);
        }

        public int GetRunDelta(int from, int fromRow, int to, int toRow)
        {
            return GetRunUp(from, fromRow) - GetRunUp(to, toRow);
        }

        public void Update(Tableau other)
        {
            for (int i = 0; i < count; i++)
            {
                upPiles[i].Update(other.upPiles[i]);
            }
        }

        public void Adjust()
        {
            for (int column = 0; column < count; column++)
            {
                CheckDiscard(column);
                CheckTurnOverCard(column);
            }
        }

        public void Move(Move move)
        {
            if (move.Type == MoveType.Basic)
            {
                Move(move.From, move.FromRow, move.To);
            }
            else if (move.Type == MoveType.Swap)
            {
                Swap(move.From, move.FromRow, move.To, move.ToRow);
            }
            else
            {
                throw new Exception("unsupported move type");
            }
        }

        public void Move(int from, int fromRow, int to)
        {
            Pile fromPile = upPiles[from];
            Pile toPile = upPiles[to];
            int fromCount = fromPile.Count - fromRow;
            toPile.AddRange(fromPile, fromRow, fromCount);
            fromPile.RemoveRange(fromRow, fromCount);
            OnPileChanged(from);
            OnPileChanged(to);
        }

        public void Swap(int from, int fromRow, int to, int toRow)
        {
            Pile fromPile = upPiles[from];
            Pile toPile = upPiles[to];
            int fromCount = fromPile.Count - fromRow;
            int toCount = toPile.Count - toRow;
            scratchPile.Clear();
            scratchPile.AddRange(toPile, toRow, toCount);
            toPile.RemoveRange(toRow, toCount);
            toPile.AddRange(fromPile, fromRow, fromCount);
            fromPile.RemoveRange(fromRow, fromCount);
            fromPile.AddRange(scratchPile, 0, toCount);
            OnPileChanged(from);
            OnPileChanged(to);
        }

        public void Add(int column, Card card)
        {
            upPiles[column].Add(card);
            OnPileChanged(column);
        }

        private void OnPileChanged(int column)
        {
            if (autoAdjust)
            {
                CheckDiscard(column);
                CheckTurnOverCard(column);
            }
        }

        private void CheckDiscard(int column)
        {
            Pile pile = upPiles[column];
            if (pile.Count < 13)
            {
                return;
            }
            if (pile[pile.Count - 1].Face != Face.Ace)
            {
                return;
            }

            int runLength = pile.GetRunUp(pile.Count);
            if (runLength == 13)
            {
                int row = pile.Count - runLength;
                Pile sequence = new Pile();
                sequence.AddRange(pile, row, 13);
                pile.RemoveRange(row, 13);
                discardPiles.Add(sequence);
            }
        }

        public void CheckTurnOverCard(int column)
        {
            Pile upPile = upPiles[column];
            Pile downPile = downPiles[column];
            if (upPile.Count == 0 && downPile.Count != 0)
            {
                upPile.Add(downPile.Next());
            }
        }

        #region IGetCard Members

        public Card GetCard(int column)
        {
            return upPiles[column].LastCard;
        }

        #endregion
    }
}
