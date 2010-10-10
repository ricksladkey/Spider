using System;
using System.Collections.Generic;
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
            BestMoves = new MoveList();
        }

        private Tableau WorkingTableau { get; set; }
        private MoveList BestMoves { get; set; }
        private double BestScore { get; set; }

        public void SearchMoves()
        {
            WorkingTableau.Variation = Variation;
            WorkingTableau.ClearAll();
            WorkingTableau.CopyUpPiles(Tableau);
            WorkingTableau.BlockDownPiles(Tableau);

            BestMoves.Clear();
            BestScore = 0;

            SearchMoves(3);

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
                ProcessSearchLeaf();
                return;
            }

            FindMoves(WorkingTableau);
            MoveList candidates = new MoveList(Candidates);
            MoveList supplementaryList = new MoveList(SupplementaryList);
            Stack<Move> moveStack = new Stack<Move>();

            if (candidates.Count == 0)
            {
                ProcessSearchLeaf();
                return;
            }

            for (int i = 0; i < candidates.Count; i++)
            {
                int timeStamp = WorkingTableau.TimeStamp;

                Move move = candidates[i];
                moveStack.Clear();
                for (int next = move.HoldingNext; next != -1; next = supplementaryList[next].Next)
                {
                    Move holdingMove = supplementaryList[next];
                    WorkingTableau.Move(holdingMove);
                    moveStack.Push(new Move(holdingMove.To, -holdingMove.ToRow, move.To));
                }
                WorkingTableau.Move(move);
                while (moveStack.Count > 0)
                {
                    Move holdingMove = moveStack.Pop();
                    if (!WorkingTableau.MoveIsValid(holdingMove))
                    {
                        break;
                    }
                    WorkingTableau.Move(holdingMove);
                }

                SearchMoves(depth - 1);

                WorkingTableau.Revert(timeStamp);
            }
        }

        private void ProcessSearchLeaf()
        {
            if (Diagnostics)
            {
                Console.WriteLine("leaf:");
                for (int i = 0; i < WorkingTableau.Moves.Count; i++)
                {
                    Console.WriteLine("move[{0}] = {1}", i, WorkingTableau.Moves[i]);
                }
            }

            double score = CalculateSearchScore();
            if (score > BestScore)
            {
                BestScore = score;
                BestMoves.Copy(WorkingTableau.Moves);
            }
        }

        public double CalculateSearchScore()
        {
            double TurnedOverCardScore = 10;
            double DiscardedScore = 100;
            double score = 0;
            for (int column = 0; column < NumberOfPiles; column++)
            {
                Pile pile = WorkingTableau[column];
                if (pile.Count == 1 && pile[0].IsEmpty)
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
