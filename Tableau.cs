using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    [DebuggerDisplay("NumberOfPiles = {NumberOfPiles}")]
    [DebuggerTypeProxy(typeof(EnumerableDebugView))]
    public class Tableau : IEnumerable<Pile>, IGetCard
    {
        public Variation Variation { get; set; }
        public bool AutoAdjust { get; set; }
        public int NumberOfPiles { get; set; }

        private Pile[] downPiles;
        private Pile[] upPiles;
        private Pile stockPile;
        private FastList<Pile> discardPiles;
        private Pile scratchPile;

        public Tableau()
        {
            AutoAdjust = true;
            Variation = Variation.Spider4;
            Initialize();
        }

        private void Initialize()
        {
            NumberOfPiles = Variation.NumberOfPiles;
            stockPile = new Pile();
            downPiles = new Pile[NumberOfPiles];
            upPiles = new Pile[NumberOfPiles];
            discardPiles = new FastList<Pile>(NumberOfPiles);
            for (int row = 0; row < NumberOfPiles; row++)
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

        public Pile StockPile
        {
            get
            {
                return stockPile;
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
            if (NumberOfPiles != Variation.NumberOfPiles)
            {
                Initialize();
            }
            stockPile.Clear();
            for (int i = 0; i < NumberOfPiles; i++)
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

        public int GetNetRunLength(int order, int from, int fromRow, int to, int toRow)
        {
            int moveRun = GetRunDown(from, fromRow);
            int fromRun = GetRunUp(from, fromRow + 1) + moveRun - 1;
            if (order != 2)
            {
                // The from card's suit doesn't match the to card's suit.
                if (moveRun == fromRun)
                {
                    // The from card's suit doesn't match its parent.
                    return 0;
                }
                return -fromRun;
            }
            int toRun = GetRunUp(to, toRow);
            int newRun = moveRun + toRun;
            if (moveRun == fromRun)
            {
                // The from card's suit doesn't match its parent.
                return newRun;
            }
            return newRun - fromRun;
        }

        public int GetNewRunLength(int order, int from, int fromRow, int to, int toRow)
        {
            if (order != 2)
            {
                // The from card's suit doesn't match the to card's suit.
                return 0;
            }
            int moveRun = GetRunDown(from, fromRow);
            int toRun = GetRunUp(to, toRow);
            int newRun = moveRun + toRun;
            return newRun;
        }

        public int GetOneRunDelta(int oldOrder, int newOrder, Move move)
        {
            bool fromFree = GetDownCount(move.From) == 0;
            bool toFree = GetDownCount(move.To) == 0;
            bool fromUpper = GetRunUp(move.From, move.FromRow) == move.FromRow;
            bool fromLower = move.HoldingNext == -1;
            bool toUpper = GetRunUp(move.To, move.ToRow) == move.ToRow;
            bool oldFrom = move.FromRow == 0 ?
                (fromFree && fromLower) :
                (fromFree && fromUpper && fromLower && oldOrder == 2);
            bool newFrom = fromFree && fromUpper;
            bool oldTo = toFree && toUpper;
            bool newTo = move.ToRow == 0 ?
                (toFree && fromLower) :
                (toFree && toUpper && fromLower && newOrder == 2);
            int oneRunDelta = (newFrom ? 1 : 0) - (oldFrom ? 1 : 0) + (newTo ? 1 : 0) - (oldTo ? 1 : 0);
            return oneRunDelta > 0 ? 1 : 0;
        }

        public void CopyUpPiles(Tableau other)
        {
            for (int i = 0; i < NumberOfPiles; i++)
            {
                upPiles[i].Copy(other.upPiles[i]);
            }
        }

        public void Adjust()
        {
            for (int column = 0; column < NumberOfPiles; column++)
            {
                CheckDiscard(column);
                CheckTurnOverCard(column);
            }
        }

        public Move Normalize(Move move)
        {
            if (move.FromRow < 0)
            {
                move.FromRow += upPiles[move.From].Count;
            }
            if (move.ToRow == -1)
            {
                move.ToRow = upPiles[move.To].Count;
            }
            return move;
        }

        public bool MoveIsValid(Move move)
        {
            return MoveIsValid(move.From, move.FromRow, move.To);
        }

        public bool MoveIsValid(int from, int fromRow, int to)
        {
            Pile fromPile = upPiles[from];
            Pile toPile = upPiles[to];
            if (fromRow < 0)
            {
                fromRow += fromPile.Count;
            }
            if (fromRow < 0 || fromRow >= fromPile.Count)
            {
                return false;
            }
            if (toPile.Count == 0)
            {
                return true;
            }
            if (!fromPile[fromRow].IsSourceFor(toPile[toPile.Count - 1]))
            {
                return false;
            }
            return true;
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

        public bool TryToMove(int from, int fromRow, int to)
        {
            if (!MoveIsValid(from, fromRow, to))
            {
                return false;
            }
            Move(from, fromRow, to);
            return true;
        }

        public void Move(int from, int fromRow, int to)
        {
            Pile fromPile = upPiles[from];
            Pile toPile = upPiles[to];

            if (fromRow < 0)
            {
                fromRow += fromPile.Count;
            }
            int fromCount = fromPile.Count - fromRow;

            Debug.Assert(fromRow >= 0 && fromRow < fromPile.Count);
            Debug.Assert(toPile.Count == 0 || fromPile[fromRow].IsSourceFor(toPile[toPile.Count - 1]));

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

        public void Deal()
        {
            for (int column = 0; column < NumberOfPiles; column++)
            {
                Add(column, stockPile.Next());
            }
        }

        public void Add(int column, Card card)
        {
            upPiles[column].Add(card);
            OnPileChanged(column);
        }

        private void OnPileChanged(int column)
        {
            if (AutoAdjust)
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

        public void PrintGame()
        {
            Game.PrintGame(new Game(this));
        }

        #region IEnumerable<Pile> Members

        public IEnumerator<Pile> GetEnumerator()
        {
            for (int column = 0; column < NumberOfPiles; column++)
            {
                yield return upPiles[column];
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IGetCard Members

        public Card GetCard(int column)
        {
            return upPiles[column].LastCard;
        }

        #endregion
    }
}
