using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    [DebuggerDisplay("NumberOfPiles = {NumberOfPiles}")]
    [DebuggerTypeProxy(typeof(EnumerableDebugView))]
    public class Tableau : BaseGame, IEnumerable<Pile>, IGetCard
    {
        static Tableau()
        {
            InitializeZobristKeys();
        }

        public Tableau()
        {
            Variation = Variation.Spider4;
            Initialize();
        }

        public Tableau(Tableau other)
            : this()
        {
            Copy(other);
        }

        public Variation Variation { get; set; }
        public int NumberOfPiles { get; private set; }
        public int NumberOfSpaces { get; private set; }
        public MoveList Moves { get; private set; }

        private Pile[] downPiles;
        private Pile[] upPiles;
        private bool[] spaceFlags;
        private Pile stockPile;
        private FastList<Pile> discardPiles;
        private FastList<int> spaces;
        private Pile scratchPile;

        private void Initialize()
        {
            NumberOfPiles = Variation.NumberOfPiles;
            Moves = new MoveList();
            stockPile = new Pile();
            downPiles = new Pile[NumberOfPiles];
            upPiles = new Pile[NumberOfPiles];
            spaceFlags = new bool[NumberOfPiles];
            discardPiles = new FastList<Pile>(NumberOfPiles);
            spaces = new FastList<int>(NumberOfPiles);
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

        public IList<int> Spaces
        {
            get
            {
                if (spaces.Count == 0 && NumberOfSpaces != 0)
                {
                    for (int column = 0; column < NumberOfPiles; column++)
                    {
                        if (spaceFlags[column])
                        {
                            spaces.Add(column);
                        }
                    }
                }
                return spaces;
            }
        }

        public void ClearAll()
        {
            Moves.Clear();
            if (NumberOfPiles != Variation.NumberOfPiles)
            {
                Initialize();
            }
            stockPile.Clear();
            for (int column = 0; column < NumberOfPiles; column++)
            {
                downPiles[column].Clear();
                upPiles[column].Clear();
            }
            discardPiles.Clear();
            spaces.Clear();
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

        public int CountSuits(int column)
        {
            return upPiles[column].CountSuits(0, -1);
        }

        public int CountSuits(int column, int row)
        {
            return upPiles[column].CountSuits(row, -1);
        }

        public int CountSuits(int column, int startRow, int endRow)
        {
            return upPiles[column].CountSuits(startRow, endRow);
        }

        public void Copy(Tableau other)
        {
            ClearAll();
            discardPiles.Copy(other.discardPiles);
            for (int column = 0; column < NumberOfPiles; column++)
            {
                upPiles[column].Copy(other.upPiles[column]);
            }
            for (int column = 0; column < NumberOfPiles; column++)
            {
                downPiles[column].Copy(other.downPiles[column]);
            }
            stockPile.Copy(other.stockPile);
            Refresh();
        }

        public void CopyUpPiles(Tableau other)
        {
            for (int column = 0; column < NumberOfPiles; column++)
            {
                upPiles[column].Copy(other.upPiles[column]);
            }
            Refresh();
        }

        public void BlockDownPiles(Tableau other)
        {
            for (int column = 0; column < NumberOfPiles; column++)
            {
                if (other.downPiles[column].Count != 0)
                {
                    downPiles[column].Add(Card.Empty);
                }
            }
        }

        public void Refresh()
        {
            int currentNumberOfSpaces = 0;
            for (int column = 0; column < NumberOfPiles; column++)
            {
                bool isEmpty = upPiles[column].Count == 0;
                spaceFlags[column] = isEmpty;
                currentNumberOfSpaces += isEmpty ? 1 : 0;
            }
            NumberOfSpaces = currentNumberOfSpaces;
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

        public bool IsValid(Move move)
        {
            if (move.Type == MoveType.Basic)
            {
                return MoveIsValid(move);
            }
            else if (move.Type == MoveType.Swap)
            {
                return SwapIsValid(move);
            }
            return false;
        }

        public bool MoveIsValid(Move move)
        {
            int from = move.From;
            int fromRow = move.FromRow;
            int to = move.To;
            int toRow = move.ToRow;

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
            if (fromPile[fromRow].IsEmpty)
            {
                return false;
            }
            if (toRow != -1 && toRow != toPile.Count)
            {
                return false;
            }
            int fromSuits = fromPile.CountSuits(fromRow);
            if (fromSuits == -1)
            {
                return false;
            }
            int numberOfSpaces = NumberOfSpaces;
            if (toPile.Count == 0)
            {
                if (numberOfSpaces == 0)
                {
                    return false;
                }
                numberOfSpaces--;
            }
            int maxExtraSuits = ExtraSuits(numberOfSpaces);
            if (fromSuits - 1 > maxExtraSuits)
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

        public bool SwapIsValid(Move move)
        {
            int from = move.From;
            int fromRow = move.FromRow;
            int to = move.To;
            int toRow = move.ToRow;

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
            if (fromPile[fromRow].IsEmpty)
            {
                return false;
            }
            int fromSuits = fromPile.CountSuits(fromRow);
            if (fromSuits == -1)
            {
                return false;
            }
            if (toRow < 0)
            {
                toRow += toPile.Count;
            }
            if (toRow < 0 || toRow >= toPile.Count)
            {
                return false;
            }
            if (toPile[toRow].IsEmpty)
            {
                return false;
            }
            int toSuits = toPile.CountSuits(toRow);
            if (toSuits == -1)
            {
                return false;
            }
            int numberOfSpaces = NumberOfSpaces;
            int maxExtraSuits = ExtraSuits(numberOfSpaces);
#if false
            if (fromSuits + toSuits - 1 > maxExtraSuits)
            {
                return false;
            }
#endif
            if (toRow != 0 && !fromPile[fromRow].IsSourceFor(toPile[toRow - 1]))
            {
                return false;
            }
            if (fromRow != 0 && !toPile[toRow].IsSourceFor(fromPile[fromRow - 1]))
            {
                return false;
            }
            return true;
        }

        public bool TryToMove(Move move)
        {
            if (!IsValid(move))
            {
                return false;
            }
            Move(move);
            return true;
        }

#if true
        public void Move(int from, int fromRow, int to)
        {
            Move(new Move(MoveType.Basic, from, fromRow, to));
        }

        public void Move(int from, int fromRow, int to, int toRow)
        {
            Move(new Move(MoveType.Swap, from, fromRow, to, toRow));
        }

        public bool MoveIsValid(int from, int fromRow, int to)
        {
            return IsValid(new Move(from, fromRow, to));
        }
#endif

        public void Move(Move move)
        {
            if (move.Type == MoveType.Basic)
            {
                DoMove(move);
            }
            else if (move.Type == MoveType.Swap)
            {
                DoSwap(move);
            }
        }

        private void DoMove(Move move)
        {
            int from = move.From;
            int fromRow = move.FromRow;
            int to = move.To;

            Pile fromPile = upPiles[from];
            Pile toPile = upPiles[to];

            if (fromRow < 0)
            {
                fromRow += fromPile.Count;
            }
            int fromCount = fromPile.Count - fromRow;
            int toRow = toPile.Count;

            Debug.Assert(fromRow >= 0 && fromRow < fromPile.Count);
            Debug.Assert(toPile.Count == 0 || fromPile[fromRow].IsSourceFor(toPile[toPile.Count - 1]));

            toPile.AddRange(fromPile, fromRow, fromCount);
            fromPile.RemoveRange(fromRow, fromCount);

            move.FromRow = fromRow;
            move.ToRow = toRow;
            Moves.Add(move);

            OnPileChanged(from);
            OnPileChanged(to);
        }

        public void UndoMove(int from, int fromRow, int to)
        {
            Pile fromPile = upPiles[from];
            Pile toPile = upPiles[to];

            int fromCount = fromPile.Count - fromRow;

            toPile.AddRange(fromPile, fromRow, fromCount);
            fromPile.RemoveRange(fromRow, fromCount);

            OnPileChanged(from);
            OnPileChanged(to);
        }

        public void DoSwap(Move move)
        {
            int from = move.From;
            int fromRow = move.FromRow;
            int to = move.To;
            int toRow = move.ToRow;

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

            Moves.Add(move);

            OnPileChanged(from);
            OnPileChanged(to);
        }

        public void UndoSwap(int from, int fromRow, int to, int toRow)
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

        public void Layout(Pile shuffled)
        {
            stockPile.AddRange(shuffled);
            foreach (LayoutPart layoutPart in Variation.LayoutParts)
            {
                for (int i = 0; i < layoutPart.Count; i++)
                {
                    int column = layoutPart.Column + i;
                    downPiles[column].Push(stockPile.Pop());
                }
            }
            Deal();
        }

        public void Deal()
        {
            if (stockPile.Count == 0)
            {
                throw new Exception("no stock left to deal");
            }
            Moves.Add(new Move(MoveType.Deal));
            for (int column = 0; column < NumberOfPiles; column++)
            {
                if (stockPile.Count == 0)
                {
                    break;
                }
                Push(column, stockPile.Pop());
            }
        }

        private void UndoDeal()
        {
            for (int column = NumberOfPiles - 1; column >= 0; column--)
            {
                stockPile.Push(Pop(column));
            }
        }

        private void Push(int column, Card card)
        {
            upPiles[column].Push(card);
            OnPileChanged(column);
        }

        private Card Pop(int column)
        {
            Card card = upPiles[column].Pop();
            OnPileChanged(column);
            return card;
        }

        private void OnPileChanged(int column)
        {
            CheckDiscard(column);
            CheckTurnOverCard(column);
            CheckSpace(column);
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
            if (runLength != 13)
            {
                return;
            }
            Discard(column);
        }

        private void Discard(int column)
        {
            Pile pile = upPiles[column];
            int row = pile.Count - 13;
            Pile sequence = new Pile();
            sequence.AddRange(pile, row, 13);
            pile.RemoveRange(row, 13);
            discardPiles.Add(sequence);
            Moves.Add(new Move(MoveType.Discard, column));
        }

        private void UndoDiscard(int column)
        {
            Pile discardPile = discardPiles.Pop();
            upPiles[column].AddRange(discardPile);
            CheckSpace(column);
        }

        private void CheckTurnOverCard(int column)
        {
            if (upPiles[column].Count == 0 && downPiles[column].Count != 0)
            {
                TurnOverCard(column);
            }
        }

        private void TurnOverCard(int column)
        {
            Pile upPile = upPiles[column];
            Pile downPile = downPiles[column];
            upPile.Push(downPile.Pop());
            Moves.Add(new Move(MoveType.TurnOverCard, column));
        }

        private void UndoTurnOverCard(int column)
        {
            downPiles[column].Push(upPiles[column].Pop());
            CheckSpace(column);
        }

        private void CheckSpace(int column)
        {
            bool isSpace = upPiles[column].Count == 0;
            if (isSpace != spaceFlags[column])
            {
                NumberOfSpaces += (isSpace ? 1 : 0) - (spaceFlags[column] ? 1 : 0);
                spaceFlags[column] = isSpace;
                spaces.Clear();
            }
        }

        public int CheckPoint
        {
            get
            {
                return Moves.Count;
            }
        }

        public void Revert(int checkPoint)
        {
            while (CheckPoint > checkPoint)
            {
                Undo();
            }
        }

        public void Undo()
        {
            Move move = Moves[Moves.Count - 1];
            switch (move.Type)
            {
                case MoveType.Basic:
                    UndoMove(move.To, move.ToRow, move.From);
                    break;

                case MoveType.Swap:
                    UndoSwap(move.From, move.FromRow, move.To, move.ToRow);
                    break;

                case MoveType.Deal:
                    UndoDeal();
                    break;

                case MoveType.Discard:
                    UndoDiscard(move.From);
                    break;

                case MoveType.TurnOverCard:
                    UndoTurnOverCard(move.From);
                    break;
            }
            Moves.Pop();
        }

        public void PrintGame()
        {
            Game.PrintGame(new Game(this));
        }

        public int GetHashKey()
        {
            int hash = 0;
            int offset = 0;
            for (int row = 0; row < discardPiles.Count; row++)
            {
                hash ^= ZobristKeys[offset][row][discardPiles[row][12].GetHashKey()];
            }
            offset++;
            for (int column = 0; column < NumberOfPiles; column++)
            {
                hash ^= GetZobristKey(column + offset, downPiles[column]);
            }
            offset += NumberOfPiles;
            for (int column = 0; column < NumberOfPiles; column++)
            {
                hash ^= GetZobristKey(column + offset, upPiles[column]);
            }
            offset += NumberOfPiles;
            hash ^= GetZobristKey(offset, stockPile);
            return hash;
        }

        public int GetUpPilesHashKey()
        {
            int hashKey = 0;
            for (int column = 0; column < NumberOfPiles; column++)
            {
                hashKey ^= GetZobristKey(column, upPiles[column]);
            }
            return hashKey;
        }

        private static int GetZobristKey(int column, Pile pile)
        {
            int hashKey = 0;
            int[][] keys = ZobristKeys[column];
            for (int row = 0; row < pile.Count; row++)
            {
                hashKey ^= keys[row][pile[row].GetHashKey()];
            }
            return hashKey;
        }

        private static int[][][] ZobristKeys;

        private static void InitializeZobristKeys()
        {
            Random random = new Random(0);
            int columns = 2 * 10 + 2;
            int rows = 52 * 2;
            int cards = 13 + 14 * 4 + 1;
            ZobristKeys = new int[columns][][];
            for (int column = 0; column < columns; column++)
            {
                ZobristKeys[column] = new int[rows][];
                for (int row = 0; row < rows; row++)
                {
                    ZobristKeys[column][row] = new int[cards];
                    for (int card = 0; card < cards; card++)
                    {
                        ZobristKeys[column][row][card] = random.Next(int.MinValue, int.MaxValue);
                    }
                }
            }
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
