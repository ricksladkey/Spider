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
        public SearchAlgorithm(Game game)
            : base(game)
        {
        }

        #region IAlgorithm Members

        public void PrepareToPlay()
        {
        }

        public void MakeMove()
        {
            SearchMoves();
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
