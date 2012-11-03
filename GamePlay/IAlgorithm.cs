using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Spider.Collections;
using Spider.Engine;

namespace Spider.GamePlay
{
    public interface IAlgorithm
    {
        void Initialize();
        IList<double> GetCoefficients();
        void SetCoefficients();
        void PrepareToPlay();
        void FindMoves(Tableau tableau);
        void MakeMove();
        void ProcessCandidate(Move move);
        void PrepareToDeal();
        void RespondToDeal();
    }
}
