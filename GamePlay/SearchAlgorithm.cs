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
    }
}
