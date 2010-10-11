using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    public class SearchMoveFinder : GameHelper
    {
        public SearchMoveFinder(Game game)
            : base(game)
        {
            WorkingTableau = new Tableau();
            TranspositionTable = new HashSet<int>();
            Moves = new MoveList();
            MaxDepth = 20;
            MaxNodes = 10000;
        }

        public Tableau WorkingTableau { get; set; }
        public HashSet<int> TranspositionTable { get; set; }
        public int MaxDepth { get; set; }
        public int MaxNodes { get; set; }
        public int NodesSearched { get; set; }
        public MoveList Moves { get; set; }
        public double Score { get; set; }

        public void SearchMoves()
        {
            WorkingTableau.Variation = Variation;
            WorkingTableau.ClearAll();
            WorkingTableau.CopyUpPiles(Tableau);
            WorkingTableau.BlockDownPiles(Tableau);

            Moves.Clear();
            Score = 0;

            TranspositionTable.Clear();
            NodesSearched = 0;
            SearchMoves(MaxDepth);
            if (NodesSearched >= MaxNodes)
            {
                int maxNodesSearched = 0;
                TranspositionTable.Clear();
                NodesSearched = 0;
                for (int depth = 1; depth < MaxDepth; depth++)
                {
                    TranspositionTable.Clear();
                    NodesSearched = 0;
                    SearchMoves(depth);
                    if (NodesSearched == maxNodesSearched)
                    {
                        break;
                    }
                    maxNodesSearched = NodesSearched;
                    if (maxNodesSearched >= MaxNodes)
                    {
                        break;
                    }
                }
            }

            if (TraceSearch)
            {
                Utils.WriteLine("Nodes searched: {0}", NodesSearched);
            }
            if (Diagnostics)
            {
                Utils.WriteLine("search: score = {0}", Score);
                for (int i = 0; i < Moves.Count; i++)
                {
                    Utils.WriteLine("search: move[{0}] = {1}", i, Moves[i]);
                }
            }

            for (int i = 0; i < Moves.Count; i++)
            {
                Move move = Moves[i];
                if (move.Type == MoveType.Basic || move.Type == MoveType.Swap)
                {
                    ProcessMove(move);
                }
                else if (move.Type == MoveType.TurnOverCard)
                {
                    break;
                }
            }
        }

        private void SearchMoves(int depth)
        {
            if (depth == 0)
            {
                return;
            }

            FindMoves(WorkingTableau);
            MoveList candidates = new MoveList(Candidates);
            MoveList supplementaryList = new MoveList(SupplementaryList);
            Stack<Move> moveStack = new Stack<Move>();

            for (int i = 0; i < candidates.Count; i++)
            {
                int timeStamp = WorkingTableau.TimeStamp;

                Move move = candidates[i];
#if false
                if (depth == 4 && move.Type == MoveType.Swap && move.From == 4 && move.FromRow == 7 && move.To == 7 && move.ToRow == 0)
                {
                    Console.WriteLine("working tableau moves:");
                    PrintMoves(WorkingTableau.Moves);
                    Debugger.Break();
                }
#endif
                bool toEmpty = move.Type == MoveType.Basic && WorkingTableau[move.To].Count == 0;
                moveStack.Clear();
                for (int next = move.HoldingNext; next != -1; next = supplementaryList[next].Next)
                {
                    Move holdingMove = supplementaryList[next];
                    WorkingTableau.Move(new Move(MoveType.Basic, MoveFlags.Holding, holdingMove.From, holdingMove.FromRow, holdingMove.To));
                    int undoTo = holdingMove.From == move.From ? move.To : move.From;
                    moveStack.Push(new Move(MoveType.Basic, MoveFlags.UndoHolding, holdingMove.To, -holdingMove.ToRow, undoTo));
                }
                WorkingTableau.Move(new Move(move.Type, move.From, move.FromRow, move.To, move.ToRow));
                if (!toEmpty)
                {
                    while (moveStack.Count > 0)
                    {
                        Move holdingMove = moveStack.Pop();
                        if (!WorkingTableau.MoveIsValid(holdingMove))
                        {
                            break;
                        }
                        WorkingTableau.Move(holdingMove);
                    }
                }

                bool continueSearch = ProcessNode();
                if (NodesSearched >= MaxNodes)
                {
                    WorkingTableau.Revert(timeStamp);
                    return;
                }
                if (continueSearch)
                {
                    SearchMoves(depth - 1);
                }
                WorkingTableau.Revert(timeStamp);
                if (NodesSearched >= MaxNodes)
                {
                    return;
                }
            }
        }

        private bool ProcessNode()
        {
            int hashCode = WorkingTableau.GetHashCode();
            if (TranspositionTable.Contains(hashCode))
            {
                return false;
            }
            TranspositionTable.Add(hashCode);

            NodesSearched++;
            double score = CalculateSearchScore();

            if (score > Score)
            {
                Score = score;
                Moves.Copy(WorkingTableau.Moves);
            }

#if false
            for (int column = 0; column < NumberOfPiles; column++)
            {
                Pile pile = WorkingTableau[column];
                if (pile.Count == 1 && pile[0].IsUnknown)
                {
                    return false;
                }
            }
#endif

            return true;
        }

        public double CalculateSearchScore()
        {
            double TurnedOverCardScore = 5;
            double SpaceScore = 1000;
            double FacesMatchScore = 1;
            double SuitsMatchScore = 2;
            double DiscardedScore = SuitsMatchScore * 12;
            double score = 0;
            for (int column = 0; column < NumberOfPiles; column++)
            {
                Pile pile = WorkingTableau[column];
                if (pile.Count == 0)
                {
                    score += SpaceScore;
                }
                else if (pile.Count == 1 && pile[0].IsUnknown)
                {
                    score += TurnedOverCardScore;
                }
                else
                {
                    for (int row = 1; row < pile.Count; row++)
                    {
                        int order = GetOrder(pile[row - 1], pile[row]);
                        if (order == 1)
                        {
                            score += FacesMatchScore;
                        }
                        else if (order == 2)
                        {
                            score += SuitsMatchScore;
                        }
                    }
                }
            }
            score += DiscardedScore * WorkingTableau.DiscardPiles.Count;
            return score;
        }
    }
}
