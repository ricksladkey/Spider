using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Spider.Engine.Collections;
using Spider.Engine.Core;

namespace Spider.Engine.GamePlay
{
    public class SearchMoveFinder : GameAdapter
    {
        private struct Node
        {
            public Node(bool continueSearch)
            {
                ContinueSearch = continueSearch;
                Moves = null;
                SupplementaryList = null;
                Nodes = null;
            }

            public Node(IList<Move> moves, IList<Move> supplementaryList)
            {
                ContinueSearch = true;
                Moves = moves;
                SupplementaryList = supplementaryList;
                Nodes = null;
            }

            public bool ContinueSearch;
            public IList<Move> Moves;
            public IList<Move> SupplementaryList;
            public IList<Node> Nodes;
        }

        public SearchMoveFinder(Game game)
            : base(game)
        {
            UseDepthFirst = false;
            WorkingTableau = new Tableau();
            TranspositionTable = new HashSet<int>();
            MoveStack = new MoveList();
            Moves = new MoveList();
            MaxDepth = 20;
            MaxNodes = 10000;
            MoveAllocator = new ListAllocator<Move>(false);
            NodeAllocator = new ListAllocator<Node>(true);
        }

        public bool UseDepthFirst { get; set; }
        public Tableau WorkingTableau { get; set; }
        public HashSet<int> TranspositionTable { get; set; }
        MoveList MoveStack { get; set; }
        public int MaxDepth { get; set; }
        public int MaxNodes { get; set; }
        public int NodesSearched { get; set; }
        public MoveList Moves { get; set; }
        public double Score { get; set; }
        private ListAllocator<Move> MoveAllocator { get; set; }
        private ListAllocator<Node> NodeAllocator { get; set; }

        public MoveList SearchMoves()
        {
            WorkingTableau.Variation = Variation;
            WorkingTableau.Clear();
            WorkingTableau.CopyUpPiles(Tableau);
            WorkingTableau.BlockDownPiles(Tableau);

            MoveAllocator.Clear();
            NodeAllocator.Clear();

            if (UseDepthFirst)
            {
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
            }
            else
            {
                StartSearch();
                Node root = new Node();
                while (true)
                {
                    int lastNodesSearched = NodesSearched;
                    BreadthFirstSearch(ref root);
                    if (NodesSearched == lastNodesSearched || NodesSearched >= MaxNodes)
                    {
                        break;
                    }
                }
            }

            if (TraceSearch)
            {
                Print();
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

            return Moves;
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

            Algorithm.FindMoves(WorkingTableau);
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

        private void BreadthFirstSearch(ref Node parent)
        {
            if (parent.Moves == null)
            {
                Algorithm.FindMoves(WorkingTableau);
                parent.Moves = new AllocatedList<Move>(MoveAllocator, Candidates);
                parent.SupplementaryList = new AllocatedList<Move>(MoveAllocator, SupplementaryList);
                parent.Nodes = new AllocatedList<Node>(NodeAllocator, parent.Moves.Count, parent.Moves.Count);
                for (int i = 0; i < parent.Moves.Count; i++)
                {
                    int checkPoint = WorkingTableau.CheckPoint;
                    MakeMove(parent, i);
                    if (ProcessNode())
                    {
                        parent.Nodes[i] = new Node(true);
                    }
                    WorkingTableau.Revert(checkPoint);
                    if (NodesSearched >= MaxNodes)
                    {
                        return;
                    }
                }
            }
            else
            {
                for (int i = 0; i < parent.Moves.Count; i++)
                {
                    Node child = parent.Nodes[i];
                    if (child.ContinueSearch)
                    {
                        int checkPoint = WorkingTableau.CheckPoint;
                        MakeMove(parent, i);
                        BreadthFirstSearch(ref child);
                        WorkingTableau.Revert(checkPoint);
                        if (NodesSearched >= MaxNodes)
                        {
                            return;
                        }
                    }
                    parent.Nodes[i] = child;
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
                    if (!WorkingTableau.IsValid(holdingMove))
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

            return true;
        }

        public double CalculateSearchScore()
        {
            double TurnedOverCardScore = Coefficients[0];
            double SpaceScore = Coefficients[1];
            double FacesMatchScore = 1;
            double SuitsMatchScore = Coefficients[2];
            double DiscardedScore = Coefficients[3];

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
                    score += TurnedOverCardScore + 5 - Tableau.GetDownCount(column);
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
