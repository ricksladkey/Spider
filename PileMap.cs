using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    [DebuggerDisplay("Count = {count}")]
    [DebuggerTypeProxy(typeof(EnumerableDebugView))]
    public class PileMap : FastList<Pile>, IGetCard
    {
        private Pile scratchPile;

        public PileMap(Tableau tableau)
            : base(tableau.NumberOfPiles, tableau.NumberOfPiles)
        {
            for (int row = 0; row < count; row++)
            {
                array[row] = new Pile();
            }
            scratchPile = new Pile();
        }

        public void ClearAll()
        {
            for (int i = 0; i < count; i++)
            {
                array[i].Clear();
            }
        }

        public int GetRunDown(int column, int row)
        {
            return array[column].GetRunDown(row);
        }

        public int GetRunDownAnySuit(int column, int row)
        {
            return array[column].GetRunDownAnySuit(row);
        }

        public int GetRunUp(int column, int row)
        {
            return array[column].GetRunUp(row);
        }

        public int GetRunUpAnySuit(int column, int row)
        {
            return array[column].GetRunUpAnySuit(row);
        }

        public int CountSuits(int column, int row)
        {
            return array[column].CountSuits(row, -1);
        }

        public int CountSuits(int column, int startRow, int endRow)
        {
            return array[column].CountSuits(startRow, endRow);
        }

        public int GetRunDelta(int from, int fromRow, int to, int toRow)
        {
            return GetRunUp(from, fromRow) - GetRunUp(to, toRow);
        }

        public void Copy(PileMap other)
        {
            for (int i = 0; i < count; i++)
            {
                array[i].Copy(other.array[i]);
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
            Pile fromPile = array[from];
            Pile toPile = array[to];
            int fromCount = fromPile.Count - fromRow;
            toPile.AddRange(fromPile, fromRow, fromCount);
            fromPile.RemoveRange(fromRow, fromCount);
            OnPileChanged(from);
            OnPileChanged(to);
        }

        public void Swap(int from, int fromRow, int to, int toRow)
        {
            Pile fromPile = array[from];
            Pile toPile = array[to];
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

        public void Discard()
        {
            for (int i = 0; i < count; i++)
            {
                Discard(i);
            }
        }

        public void Discard(int column)
        {
            Pile pile = array[column];
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
                OnDiscard(sequence);
            }
        }

        public event Action<int> PileChangedEvent;

        protected void OnPileChanged(int column)
        {
            if (PileChangedEvent != null)
            {
                PileChangedEvent(column);
            }
        }

        public event Action<Pile> DiscardEvent;

        protected void OnDiscard(Pile sequence)
        {
            if (DiscardEvent != null)
            {
                DiscardEvent(sequence);
            }
        }

        #region IGetCard Members

        public Card GetCard(int column)
        {
            return array[column].LastCard;
        }

        #endregion
    }
}
