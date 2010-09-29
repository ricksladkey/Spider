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

        public PileMap()
            : base(Game.NumberOfPiles, Game.NumberOfPiles)
        {
            for (int row = 0; row < Game.NumberOfPiles; row++)
            {
                array[row] = new Pile();
            }
            scratchPile = new Pile();
        }

        public void ClearAll()
        {
            for (int i = 0; i < Game.NumberOfPiles; i++)
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

        public void Update(PileMap other)
        {
            for (int i = 0; i < count; i++)
            {
                array[i].Update(other.array[i]);
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
        }

        #region IGetCard Members

        public Card GetCard(int column)
        {
            return array[column].LastCard;
        }

        #endregion
    }
}
