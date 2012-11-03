using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Spider.Collections;
using Spider.Engine;

namespace Spider.GamePlay
{
    public interface IGame : IGameSettings
    {
        Pile Shuffled { get; }
        Tableau Tableau { get; }
        Tableau FindTableau { get; }
        IAlgorithm Algorithm { get; }
        int NumberOfPiles { get; }
        int NumberOfSuits { get; }

        MoveList Candidates { get; }
        MoveList SupplementaryList { get; }
        PileList[] FaceLists { get; }
        HoldingStack[] HoldingStacks { get; }
        RunFinder RunFinder { get; }

        void PrepareToFindMoves(Tableau tableau);
        void ProcessMove(Move move);
        int AddHolding(HoldingSet holdingSet);
        int AddHolding(HoldingSet holdingSet1, HoldingSet holdingSet2);
        int AddSupplementary(MoveList supplementaryMoves);
        int FindHolding(IGetCard map, HoldingStack holdingStack, bool inclusive, Pile fromPile, int from, int fromStart, int fromEnd, int to, int maxExtraSuits);
        void Print();
        void PrintBeforeAndAfter();
        void PrintMoves();
        void PrintMoves(MoveList moves);
        void PrintCandidates();
        void PrintViableCandidates();
        void PrintMove(Move move);
    }
}
