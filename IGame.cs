using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public interface IGame : IGameSettings
    {
        MoveList Moves { get; }

        Pile Shuffled { get; }
        Tableau Tableau { get; }
        int NumberOfPiles { get; }
        int NumberOfSuits { get; }

        MoveList UncoveringMoves { get; }
        MoveList SupplementaryMoves { get; }
        MoveList SupplementaryList { get; }
        MoveList Candidates { get; }
        PileList OneRunPiles { get; }
        List<HoldingInfo> HoldingList { get; }

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
