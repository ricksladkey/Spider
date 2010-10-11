using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    class SearchMoveFinder : GameHelper
    {
        public SearchMoveFinder(Game game)
            : base(game)
        {
            WorkingTableau = new Tableau();
            TranspositionTable = new HashSet<int>();
            BestMoves = new MoveList();
        }

        private Tableau WorkingTableau { get; set; }
        private HashSet<int> TranspositionTable { get; set; }
        private MoveList BestMoves { get; set; }
        private double BestScore { get; set; }
        private int NodesSearched { get; set; }

        public void SearchMoves()
        {
            WorkingTableau.Variation = Variation;
            WorkingTableau.ClearAll();
            WorkingTableau.CopyUpPiles(Tableau);
            WorkingTableau.BlockDownPiles(Tableau);

            TranspositionTable.Clear();
            BestMoves.Clear();
            BestScore = 0;
            NodesSearched = 0;

            SearchMoves(6);

            if (TraceSearch)
            {
                Utils.WriteLine("Nodes searched: {0}", NodesSearched);
            }
            if (Diagnostics)
            {
                Utils.WriteLine("best: score = {0}", BestScore);
                for (int i = 0; i < BestMoves.Count; i++)
                {
                    Utils.WriteLine("best: move[{0}] = {1}", i, BestMoves[i]);
                }
            }

            for (int i = 0; i < BestMoves.Count; i++)
            {
                Move move = BestMoves[i];
                if (move.Type == MoveType.Basic || move.Type == MoveType.Swap)
                {
                    ProcessMove(move);
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
                bool toEmpty = move.Type == MoveType.Basic && WorkingTableau[move.To].Count == 0;
                moveStack.Clear();
                for (int next = move.HoldingNext; next != -1; next = supplementaryList[next].Next)
                {
                    Move holdingMove = supplementaryList[next];
                    WorkingTableau.Move(holdingMove);
                    int undoTo = holdingMove.From == move.From ? move.To : move.From;
                    moveStack.Push(new Move(holdingMove.To, -holdingMove.ToRow, undoTo));
                }
                WorkingTableau.Move(move);
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

                if (ProcessNode())
                {
                    SearchMoves(depth - 1);
                }

                WorkingTableau.Revert(timeStamp);
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

            if (score > BestScore)
            {
                BestScore = score;
                BestMoves.Copy(WorkingTableau.Moves);
            }

            return true;
        }

        public double CalculateSearchScore()
        {
            double TurnedOverCardScore = 10;
            double SpaceScore = 25;
            double DiscardedScore = 100;
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
                        score += GetOrder(pile[row - 1], pile[row]);
                    }
                }
            }
            score += DiscardedScore * WorkingTableau.DiscardPiles.Count;
            return score;
        }
    }
}
