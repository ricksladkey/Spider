using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public class RunFinder : BaseGame
    {
        private class PileInfo
        {
            public int Count;
            public int RunLength;
            public int RunLengthAnySuit;
            public RunInfo[] RunInfoArray;
        }

        private struct RunInfo
        {
            public RunInfo(int startRow, int endRow, int suits)
            {
                StartRow = startRow;
                EndRow = endRow;
                Length = endRow - startRow;
                Suits = suits;
            }

            public int StartRow;
            public int EndRow;
            public int Length;
            public int Suits;
        }

        private const int MaxColumns = 10;
        private const int MaxRows = 52 * 2 + 1;

        public RunFinder()
        {
            this.pileInfoArray = new PileInfo[MaxColumns];
            for (int column = 0; column < MaxColumns; column++)
            {
                pileInfoArray[column] = new PileInfo();
                pileInfoArray[column].RunInfoArray = new RunInfo[MaxRows];
            }
        }

        private Tableau tableau;
        private PileInfo[] pileInfoArray;

        public void Find(Tableau tableau)
        {
            this.tableau = tableau;

            int n = tableau.NumberOfPiles;
            for (int column = 0; column < n; column++)
            {
                Pile pile = tableau[column];
                PileInfo pileInfo = pileInfoArray[column];
                int m = pile.Count;
                pileInfo.Count = m;
                pileInfo.RunLength = pile.GetRunUp(m);
                pileInfo.RunLengthAnySuit = pile.GetRunUpAnySuit(m);
                pileInfo.RunInfoArray[m] = new RunInfo(m, m, 0);
                if (m == 0)
                {
                    pileInfo.RunLength = 0;
                    pileInfo.RunLengthAnySuit = 0;
                    continue;
                }
                if (m == 1)
                {
                    pileInfo.RunLength = 1;
                    pileInfo.RunLengthAnySuit = 1;
                    pileInfo.RunInfoArray[0] = new RunInfo(0, 1, 1);
                    continue;
                }

                RunInfo[] runInfoArray = pileInfo.RunInfoArray;
                int startRow = m - 1;
                int endRow = m;
                int suits = 1;
                Card previousCard = pile[endRow - 1];
                for (int currentRow = m - 2; currentRow >= 0; currentRow--)
                {
                    Card currentCard = pile[currentRow];
                    int order = GetOrder(currentCard, previousCard);
                    if (order == 2)
                    {
                        startRow = currentRow;
                    }
                    else
                    {
                        for (int row = startRow; row < endRow; row++)
                        {
                            runInfoArray[row] = new RunInfo(startRow, endRow, suits);
                        }
                        startRow = currentRow;
                        endRow = currentRow + 1;
                        if (order == 0)
                        {
                            suits = -1;
                        }
                        else if (suits != -1)
                        {
                            suits++;
                        }
                    }
                    previousCard = currentCard;
                }
                for (int row = startRow; row < endRow; row++)
                {
                    runInfoArray[row] = new RunInfo(startRow, endRow, suits);
                }
            }
        }

        public int GetRunLength(int column)
        {
            return pileInfoArray[column].RunLength;

        }

        public int GetRunLengthAnySuit(int column)
        {
            return pileInfoArray[column].RunLengthAnySuit;
        }

        public int GetRunUp(int column, int row)
        {
            if (row == 0)
            {
                return 0;
            }
            int result = row - pileInfoArray[column].RunInfoArray[row - 1].StartRow;
            Debug.Assert(result == tableau.GetRunUp(column, row));
            return result;
        }

        public int GetRunDown(int column, int row)
        {
            int result = pileInfoArray[column].RunInfoArray[row].EndRow - row;
            Debug.Assert(result == tableau.GetRunDown(column, row));
            return result;
        }

        public int CountSuits(int column)
        {
            int result = pileInfoArray[column].RunInfoArray[0].Suits;
            Debug.Assert(result == tableau.CountSuits(column));
            return result;
        }

        public int CountSuits(int column, int row)
        {
            int result = pileInfoArray[column].RunInfoArray[row].Suits;
            Debug.Assert(result == tableau.CountSuits(column, row));
            return result;
        }

        public int CountSuits(int column, int startRow, int endRow)
        {
            int result = tableau.CountSuits(column, startRow, endRow);
            Debug.Assert(result == tableau.CountSuits(column, startRow, endRow));
            return result;
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
            bool fromFree = tableau.GetDownCount(move.From) == 0;
            bool toFree = tableau.GetDownCount(move.To) == 0;
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
    }
}
