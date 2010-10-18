using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Spider.Collections;
using Spider.Engine;

namespace Spider.GamePlay
{
    public class SearchAlgorithm : GameAdapter, IAlgorithm
    {
        public static double[] SearchCoefficients = new double[] {
            5, 1000, 2, 24
        };

        public SearchAlgorithm(Game game)
            : base(game)
        {
            BasicMoveFinder = new BasicMoveFinder(game);
            SwapMoveFinder = new SwapMoveFinder(game);
            SearchMoveFinder = new SearchMoveFinder(game);
        }

        private BasicMoveFinder BasicMoveFinder { get; set; }
        private SwapMoveFinder SwapMoveFinder { get; set; }
        private SearchMoveFinder SearchMoveFinder { get; set; }

        #region IAlgorithm Members

        public void SetCoefficients()
        {
            SetDefaultCoefficients(SearchCoefficients);
        }

        public void PrepareToPlay()
        {
        }

        public void FindMoves(Tableau tableau)
        {
            PrepareToFindMoves(tableau);
            BasicMoveFinder.Find();
            SwapMoveFinder.Find();
        }

        public void MakeMove()
        {
            Algorithm.FindMoves(Tableau);
            int best = -1;
            for (int i = 0; i < Candidates.Count; i++)
            {
                Move move = Candidates[i];
                if (IsReversible(move))
                {
                    if (best == -1 || move.Score > Candidates[best].Score)
                    {
                        best = i;
                    }
                }
            }
            if (best != -1)
            {
                ProcessMove(Candidates[best]);
                return;
            }

            MoveList moves = SearchMoveFinder.SearchMoves();

            for (int i = 0; i < moves.Count; i++)
            {
                Move move = moves[i];
                if (move.Type == MoveType.Basic || move.Type == MoveType.Swap)
                {
                    ProcessMove(move);
                }
                else if (move.Type == MoveType.TurnOverCard)
                {
                    // New information.
                    break;
                }
            }
        }

        public void ProcessCandidate(Move move)
        {
            if (IsViable(move))
            {
                Candidates.Add(move);
            }
        }

        public void PrepareToDeal()
        {
        }

        public void RespondToDeal()
        {
        }

        #endregion

        public bool IsReversible(Move move)
        {
            int from = move.From;
            int fromRow = move.FromRow;
            int to = move.To;
            int toRow = move.ToRow;
            Pile fromPile = FindTableau[from];
            Pile toPile = FindTableau[to];
            bool isSwap = move.Type == MoveType.Swap;
            Card fromParent = fromRow != 0 ? fromPile[fromRow - 1] : Card.Empty;
            Card fromChild = fromPile[fromRow];
            Card toParent = toRow != 0 ? toPile[toRow - 1] : Card.Empty;
            Card toChild = toRow != toPile.Count ? toPile[toRow] : Card.Empty;
            int oldOrderFrom = GetOrder(fromParent, fromChild);
            int newOrderFrom = GetOrder(toParent, fromChild);
            int oldOrderTo = isSwap ? GetOrder(toParent, toChild) : 0;
            int newOrderTo = isSwap ? GetOrder(fromParent, toChild) : 0;
            return oldOrderFrom != 0 && (!isSwap || oldOrderTo != 0);
        }

        public bool IsViable(Move move)
        {
            int from = move.From;
            int fromRow = move.FromRow;
            int to = move.To;
            int toRow = move.ToRow;

            Pile fromPile = FindTableau[from];
            Pile toPile = FindTableau[to];
            if (toPile.Count == 0)
            {
                if (fromPile.Count == 0 && FindTableau.GetDownCount(from) == 0)
                {
                    return false;
                }
                else if (fromRow != 0 && fromPile[fromRow - 1].IsTargetFor(fromPile[fromRow]))
                {
                    return false;
                }
                return true;
            }
            bool isSwap = move.Type == MoveType.Swap;
            Card fromParent = fromRow != 0 ? fromPile[fromRow - 1] : Card.Empty;
            Card fromChild = fromPile[fromRow];
            Card toParent = toRow != 0 ? toPile[toRow - 1] : Card.Empty;
            Card toChild = toRow != toPile.Count ? toPile[toRow] : Card.Empty;
            int oldOrderFrom = GetOrder(fromParent, fromChild);
            int newOrderFrom = GetOrder(toParent, fromChild);
            int oldOrderTo = isSwap ? GetOrder(toParent, toChild) : 0;
            int newOrderTo = isSwap ? GetOrder(fromParent, toChild) : 0;
            int order = newOrderFrom - oldOrderFrom + newOrderTo - oldOrderTo;
            if (order < 0)
            {
                return false;
            }
            int netRunLengthFrom = RunFinder.GetNetRunLength(newOrderFrom, from, fromRow, to, toRow);
            int netRunLengthTo = isSwap ? RunFinder.GetNetRunLength(newOrderTo, to, toRow, from, fromRow) : 0;
            int netRunLength = netRunLengthFrom + netRunLengthTo;
            if (order == 0 && netRunLength < 0)
            {
                return false;
            }
            int delta = 0;
            if (order == 0 && netRunLength == 0)
            {
                if (!isSwap && oldOrderFrom == 1 && newOrderFrom == 1)
                {
                    delta = RunFinder.GetRunDelta(from, fromRow, to, toRow);
                }
                if (delta <= 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
