using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public interface IGame : IGameSettings
    {
        Pile Shuffled { get; }
        Tableau Tableau { get; }
        Tableau FindTableau { get; }
        int NumberOfPiles { get; }
        int NumberOfSuits { get; }

        MoveList UncoveringMoves { get; }
        MoveList SupplementaryMoves { get; }
        MoveList SupplementaryList { get; }
        MoveList Candidates { get; }
        PileList OneRunPiles { get; }
        HoldingStack[] HoldingStacks { get; }
        RunFinder RunFinder { get; }

        void FindMoves(Tableau tableau);
        void ProcessCandidate(Move move);
        void ProcessMove(Move move);
        int AddHolding(HoldingSet holdingSet);
        int AddHolding(HoldingSet holdingSet1, HoldingSet holdingSet2);
        int AddSupplementary();
        int FindHolding(IGetCard map, HoldingStack holdingStack, bool inclusive, Pile fromPile, int from, int fromStart, int fromEnd, int to, int maxExtraSuits);
        void PrintGame();
        void PrintGames();
        void PrintMoves();
        void PrintMoves(MoveList moves);
        void PrintCandidates();
        void PrintViableCandidates();
        void PrintMove(Move move);
    }
}
