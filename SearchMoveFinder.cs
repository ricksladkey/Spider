using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    public class SearchMoveFinder : GameHelper
    {
        private class Node
        {
            public Node()
            {
            }

            public Node(MoveList moves, MoveList supplementaryList)
            {
                Moves = moves;
                SupplementaryList = supplementaryList;
            }

            public MoveList Moves { get; set; }
            public MoveList SupplementaryList { get; set; }
            public FastList<Node> Nodes { get; set; }
        }

        public SearchMoveFinder(Game game)
            : base(game)
        {
            WorkingTableau = new Tableau();
            TranspositionTable = new HashSet<int>();
            MoveStack = new MoveList();
            Moves = new MoveList();
            MaxDepth = 20;
            MaxNodes = 10000;
        }

        public Tableau WorkingTableau { get; set; }
        public HashSet<int> TranspositionTable { get; set; }
        MoveList MoveStack { get; set; }
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

#if false
            StartSearch();
            DepthFirstSearch(MaxDepth);
            if (NodesSearched >= MaxNodes)
            {
                int maxNodesSearched = 0;
                for (int depth = 1; depth < MaxDepth; depth++)
                {
                    StartSearch();
                    DepthFirstSearch(depth);
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
#else
            StartSearch();
            Node root = new Node();
            while (true)
            {
                int lastNodesSearched = NodesSearched;
                BreadthFirstSearch(root);
                if (lastNodesSearched == NodesSearched || lastNodesSearched >= MaxNodes)
                {
                    break;
                }
            }
#endif

            if (TraceSearch)
            {
                PrintGame();
                Utils.WriteLine("search: score = {0}", Score);
                for (int i = 0; i < Moves.Count; i++)
                {
                    Utils.WriteLine("search: move[{0}] = {1}", i, Moves[i]);
                }
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
                    // New information.
                    break;
                }
            }
        }

        private void StartSearch()
        {
            TranspositionTable.Clear();
            ProcessNode();
            NodesSearched = 0;
            Moves.Clear();
            Score = 0;
        }

        private void DepthFirstSearch(int depth)
        {
            if (depth == 0)
            {
                return;
            }

            FindMoves(WorkingTableau);
            Node node = new Node(new MoveList(Candidates), new MoveList(SupplementaryList));

            for (int i = 0; i < node.Moves.Count; i++)
            {
                int checkPoint = WorkingTableau.CheckPoint;

                MakeMove(node, i);

                bool continueSearch = ProcessNode();
                if (NodesSearched >= MaxNodes)
                {
                    WorkingTableau.Revert(checkPoint);
                    return;
                }
                if (continueSearch)
                {
                    DepthFirstSearch(depth - 1);
                }
                WorkingTableau.Revert(checkPoint);
                if (NodesSearched >= MaxNodes)
                {
                    return;
                }
            }
        }

        private void BreadthFirstSearch(Node parent)
        {
            if (parent.Moves == null)
            {
                FindMoves(WorkingTableau);
                parent.Moves = new MoveList(Candidates);
                parent.SupplementaryList = new MoveList(SupplementaryList);
                parent.Nodes = new FastList<Node>();
                for (int i = 0; i < parent.Moves.Count; i++)
                {
                    Node child = null;
                    int checkPoint = WorkingTableau.CheckPoint;
                    MakeMove(parent, i);
                    if (ProcessNode())
                    {
                        child = new Node();
                    }
                    WorkingTableau.Revert(checkPoint);
                    if (NodesSearched >= MaxNodes)
                    {
                        return;
                    }
                    parent.Nodes.Add(child);
                }
            }
            else
            {
                for (int i = 0; i < parent.Moves.Count; i++)
                {
                    Node child = parent.Nodes[i];
                    if (child != null)
                    {
                        int checkPoint = WorkingTableau.CheckPoint;
                        MakeMove(parent, i);
                        BreadthFirstSearch(child);
                        WorkingTableau.Revert(checkPoint);
                        if (NodesSearched >= MaxNodes)
                        {
                            return;
                        }
                    }
                }
            }
        }

        private void MakeMove(Node node, int i)
        {
            Move move = node.Moves[i];
            bool toEmpty = move.Type == MoveType.Basic && WorkingTableau[move.To].Count == 0;
            MoveStack.Clear();
            for (int next = move.HoldingNext; next != -1; next = node.SupplementaryList[next].Next)
            {
                Move holdingMove = node.SupplementaryList[next];
                WorkingTableau.Move(new Move(MoveType.Basic, MoveFlags.Holding, holdingMove.From, holdingMove.FromRow, holdingMove.To));
                int undoTo = holdingMove.From == move.From ? move.To : move.From;
                MoveStack.Push(new Move(MoveType.Basic, MoveFlags.UndoHolding, holdingMove.To, -holdingMove.ToRow, undoTo));
            }
            WorkingTableau.Move(new Move(move.Type, move.From, move.FromRow, move.To, move.ToRow));
            if (!toEmpty)
            {
                while (MoveStack.Count > 0)
                {
                    Move holdingMove = MoveStack.Pop();
                    if (!WorkingTableau.MoveIsValid(holdingMove))
                    {
                        break;
                    }
                    WorkingTableau.Move(holdingMove);
                }
            }
        }

        private bool ProcessNode()
        {
            int hashKey = WorkingTableau.GetUpPilesHashKey();
            if (TranspositionTable.Contains(hashKey))
            {
                return false;
            }
            TranspositionTable.Add(hashKey);

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
                if (pile.Count == 1 && pile[0].IsEmpty)
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
                else if (pile.Count == 1 && pile[0].IsEmpty)
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
