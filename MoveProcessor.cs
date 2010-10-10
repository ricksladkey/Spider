using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    public class MoveProcessor : GameHelper
    {
        public MoveProcessor(Game game)
            : base(game)
        {
        }

        public void ProcessMove(Move move)
        {
            if (ComplexMoves)
            {
                MakeMove(move);
            }
            else
            {
                ConvertToSimpleMoves(move);
            }
        }

        public void ConvertToSimpleMoves(Move move)
        {
            if (Diagnostics)
            {
                Utils.WriteLine("CTSM: {0}", move);
            }

            // First move to the holding piles.
            Stack<Move> moveStack = new Stack<Move>();
            for (int holdingNext = move.HoldingNext; holdingNext != -1; holdingNext = SupplementaryList[holdingNext].Next)
            {
                Move holdingMove = SupplementaryList[holdingNext];
                int undoFromRow = Tableau[holdingMove.To].Count;
                MakeMoveUsingSpaces(holdingMove.From, holdingMove.FromRow, holdingMove.To);
                moveStack.Push(new Move(holdingMove.To, undoFromRow, holdingMove.From == move.From ? move.To : move.From));
            }
            if (move.Type == MoveType.CompositeSinglePile)
            {
                // Composite single pile move.
                MakeCompositeSinglePileMove(move.Next);
            }
            else if (move.Type == MoveType.Swap)
            {
                // Swap move.
                SwapUsingSpaces(move.From, move.FromRow, move.To, move.ToRow);
            }
            else
            {
                // Ordinary move.
                MakeMoveUsingSpaces(move.From, move.FromRow, move.To);
            }

            // Lastly move from the holding piles, if we still can.
            while (moveStack.Count > 0)
            {
                TryToMakeMoveUsingSpaces(moveStack.Pop());
            }
        }

        public void MakeMove(Move move)
        {
            if (move.Next != -1)
            {
                for (int next = move.Next; next != -1; next = SupplementaryList[next].Next)
                {
                    Move subMove = SupplementaryList[next];
                    MakeSingleMove(subMove);
                }
                return;
            }
            MakeSingleMove(move);
        }

        private void SwapUsingSpaces(int from, int fromRow, int to, int toRow)
        {
            if (Diagnostics)
            {
                Utils.WriteLine("SWUS: {0}/{1} -> {2}/{3}", from, fromRow, to, toRow);
            }
            int fromSuits = Tableau.CountSuits(from, fromRow);
            int toSuits = Tableau.CountSuits(to, toRow);
            if (fromSuits == 0 && toSuits == 0)
            {
                return;
            }
            if (fromSuits == 0)
            {
                MakeMoveUsingSpaces(to, toRow, from);
                return;
            }
            if (toSuits == 0)
            {
                MakeMoveUsingSpaces(from, fromRow, to);
                return;
            }
            int numberOfSpaces = Tableau.NumberOfSpaces;
            PileList spaces = new PileList(Tableau.Spaces);
            if (fromSuits + toSuits - 1 > ExtraSuits(numberOfSpaces))
            {
                throw new InvalidMoveException("insufficient spaces");
            }
            Stack<Move> moveStack = new Stack<Move>();
            for (int n = numberOfSpaces; n > 0 && fromSuits + toSuits > 1; n--)
            {
                if (fromSuits >= toSuits)
                {
                    int moveSuits = toSuits != 0 ? fromSuits : fromSuits - 1;
                    fromSuits -= MoveOffUsingSpaces(from, fromRow, to, moveSuits, n, spaces, moveStack);
                }
                else
                {
                    int moveSuits = fromSuits != 0 ? toSuits : toSuits - 1;
                    toSuits -= MoveOffUsingSpaces(to, toRow, from, moveSuits, n, spaces, moveStack);
                }
            }
            if (fromSuits + toSuits != 1 || fromSuits * toSuits != 0)
            {
                throw new Exception("bug: left over swap runs");
            }
            if (fromSuits == 1)
            {
                MakeSimpleMove(from, fromRow, to);
            }
            else
            {
                MakeSimpleMove(to, toRow, from);
            }
            while (moveStack.Count != 0)
            {
                Move move = moveStack.Pop();
                MakeSimpleMove(move.From, move.FromRow, move.To);
            }
        }

        private void UnloadToSpaces(int from, int fromRow, int to, Stack<Move> moveStack)
        {
            if (Diagnostics)
            {
                Utils.WriteLine("ULTS: {0}/{1} -> {2}", from, fromRow, to);
            }
            int numberOfSpaces = Tableau.NumberOfSpaces;
            int suits = Tableau.CountSuits(from, fromRow);
            if (suits > ExtraSuits(numberOfSpaces))
            {
                throw new InvalidMoveException("insufficient spaces");
            }
            PileList spaces = new PileList(Tableau.Spaces);
            int totalSuits = Tableau.CountSuits(from, fromRow);
            int remainingSuits = totalSuits;
            int currrentFromRow = Tableau[from].Count;
            for (int n = 0; n < numberOfSpaces; n++)
            {
                int m = Math.Min(numberOfSpaces, n + remainingSuits);
                for (int i = m - 1; i >= n; i--)
                {
                    int runLength = Tableau.GetRunUp(from, currrentFromRow);
                    currrentFromRow -= runLength;
                    currrentFromRow = Math.Max(currrentFromRow, fromRow);
                    MakeSimpleMove(from, -runLength, spaces[i]);
                    moveStack.Push(new Move(spaces[i], -runLength, to));
                    remainingSuits--;
                }
                for (int i = n + 1; i < m; i++)
                {
                    int runLength = Tableau[spaces[i]].Count;
                    MakeSimpleMove(spaces[i], -runLength, spaces[n]);
                    moveStack.Push(new Move(spaces[n], -runLength, spaces[i]));
                }
                if (remainingSuits == 0)
                {
                    break;
                }
            }
        }

        private int MoveOffUsingSpaces(int from, int fromRow, int to, int remainingSuits, int n, PileList spaces, Stack<Move> moveStack)
        {
            int suits = Math.Min(remainingSuits, n);
            if (Diagnostics)
            {
                Utils.WriteLine("MOUS: {0} -> {1}: {2}", from, to, suits);
            }
            for (int i = n - suits; i < n; i++)
            {
                // Move as much as possible but not too much.
                Pile fromPile = Tableau[from];
                int currentFromRow = fromPile.Count - Tableau.GetRunUp(from, fromPile.Count);
                if (currentFromRow < fromRow)
                {
                    currentFromRow = fromRow;
                }
                int runLength = fromPile.Count - currentFromRow;
                MakeSimpleMove(from, -runLength, spaces[i]);
                moveStack.Push(new Move(spaces[i], -runLength, to));
            }
            for (int i = n - 2; i >= n - suits; i--)
            {
                int runLength = Tableau[spaces[i]].Count;
                MakeSimpleMove(spaces[i], -runLength, spaces[n - 1]);
                moveStack.Push(new Move(spaces[n - 1], -runLength, spaces[i]));
            }
            return suits;
        }

        private void MakeCompositeSinglePileMove(int first)
        {
            if (Diagnostics)
            {
                Utils.WriteLine("MCSPM");
            }
            bool aborted = false;
            int offloadPile = -1;
            Stack<Move> moveStack = new Stack<Move>();
            for (int next = first; next != -1; next = SupplementaryList[next].Next)
            {
                int numberOfSpaces = Tableau.NumberOfSpaces;
                Move move = Tableau.Normalize(SupplementaryList[next]);
                if (move.Type == MoveType.Unload)
                {
                    offloadPile = move.To;
                    UnloadToSpaces(move.From, move.FromRow, -1, moveStack);
                }
                else if (move.Type == MoveType.Reload)
                {
                    if (Diagnostics)
                    {
                        Utils.WriteLine("RL:");
                    }
                    while (moveStack.Count != 0)
                    {
                        Move subMove = moveStack.Pop();
                        int to = subMove.To != -1 ? subMove.To : move.To;
                        MakeSimpleMove(subMove.From, subMove.FromRow, to);
                    }
                    offloadPile = -1;

                }
                else if (move.Flags.UndoHolding())
                {
                    TryToMakeMoveUsingSpaces(move);
                }
                else
                {
                    if (!TryToMakeMoveUsingSpaces(move))
                    {
                        // Things got messed up due to a discard.  There might
                        // be another pile with the same target.
                        bool foundAlternative = false;
                        Pile fromPile = Tableau[move.From];
                        if (move.From >= 0 && move.From < fromPile.Count)
                        {
                            Card fromCard = fromPile[move.FromRow];
                            for (int to = 0; to < NumberOfPiles; to++)
                            {
                                if (to == move.From)
                                {
                                    continue;
                                }
                                Pile toPile = Tableau[to];
                                if (toPile.Count == 0)
                                {
                                    continue;
                                }
                                if (!fromCard.IsSourceFor(toPile[toPile.Count - 1]))
                                {
                                    continue;
                                }
                                if (TryToMakeMoveUsingSpaces(new Move(move.From, move.FromRow, to)))
                                {
                                    foundAlternative = true;
                                }
                                break;
                            }
                        }
                        if (!foundAlternative)
                        {
                            // This move is hopelessly messed up.
                            aborted = true;
                            break;
                        }
                    }
                }
            }
            if (!aborted && moveStack.Count != 0)
            {
                throw new Exception("missing reload move");
            }
        }

        private bool TryToMakeMoveUsingSpaces(Move move)
        {
            if (Diagnostics)
            {
                Utils.WriteLine("TTMMUS: {0}/{1} -> {2}", move.From, move.FromRow, move.To);
            }
            if (SimpleMoveIsValid(move))
            {
                if (SafeMakeMoveUsingSpaces(move.From, move.FromRow, move.To) == null)
                {
                    return true;
                }
            }
            if (Diagnostics)
            {
                Utils.WriteLine("*** failed to make move ***");
            }
            return false;
        }

        private bool SimpleMoveIsValid(Move move)
        {
            return Tableau.MoveIsValid(move);
        }

        private void MakeMovesUsingSpaces(int first)
        {
            for (int next = first; next != -1; next = SupplementaryList[next].Next)
            {
                Move move = SupplementaryList[next];
                MakeMoveUsingSpaces(move.From, move.FromRow, move.To);
            }
        }

        private void MakeMoveUsingSpaces(int from, int fromRow, int to)
        {
            string error = SafeMakeMoveUsingSpaces(from, fromRow, to);
            if (error != null)
            {
                throw new InvalidMoveException(error);
            }
        }

        private string SafeMakeMoveUsingSpaces(int from, int fromRow, int to)
        {
            if (fromRow < 0)
            {
                fromRow += Tableau[from].Count;
            }
            if (Diagnostics)
            {
                Utils.WriteLine("MMUS: {0}/{1} -> {2}", from, fromRow, to);
            }
            int toRow = Tableau[to].Count;
            int extraSuits = Tableau.CountSuits(from, fromRow) - 1;
            if (extraSuits < 0)
            {
                return "not a single run";
            }
            if (extraSuits == 0)
            {
                MakeSimpleMove(from, fromRow, to);
                return null;
            }
            int numberOfSpaces = Tableau.NumberOfSpaces;
            PileList spaces = new PileList(Tableau.Spaces);
            if (toRow == 0)
            {
                spaces.Remove(to);
                numberOfSpaces--;
            }
            int maxExtraSuits = ExtraSuits(numberOfSpaces);
            if (extraSuits > maxExtraSuits)
            {
                return "insufficient spaces";
            }
            int suits = 0;
            int currentFromRow = Tableau[from].Count;
            Stack<Move> moveStack = new Stack<Move>();
            for (int n = numberOfSpaces; n > 0; n--)
            {
                for (int i = 0; i < n; i++)
                {
                    int runLength = Tableau.GetRunUp(from, currentFromRow);
                    currentFromRow -= runLength;
                    MakeSimpleMove(from, -runLength, spaces[i]);
                    moveStack.Push(new Move(spaces[i], -runLength, to));
                    suits++;
                    if (suits == extraSuits)
                    {
                        break;
                    }
                }
                if (suits == extraSuits)
                {
                    break;
                }
                for (int i = n - 2; i >= 0; i--)
                {
                    int runLength = Tableau[spaces[i]].Count;
                    MakeSimpleMove(spaces[i], -runLength, spaces[n - 1]);
                    moveStack.Push(new Move(spaces[n - 1], -runLength, spaces[i]));
                }
            }
            MakeSimpleMove(from, fromRow, to);
            while (moveStack.Count != 0)
            {
                Move move = moveStack.Pop();
                MakeSimpleMove(move.From, move.FromRow, move.To);
            }
            return null;
        }

        private void MakeSimpleMove(int from, int fromRow, int to)
        {
            if (fromRow < 0)
            {
                fromRow += Tableau[from].Count;
            }
            if (Diagnostics)
            {
                Utils.WriteLine("    MSM: {0}/{1} -> {2}", from, fromRow, to);
            }
            Debug.Assert(Tableau[from].Count != 0);
            Debug.Assert(fromRow < Tableau[from].Count);
            Debug.Assert(Tableau.CountSuits(from, fromRow) == 1);
            Debug.Assert(Tableau[to].Count == 0 || Tableau[from][fromRow].IsSourceFor(Tableau[to][Tableau[to].Count - 1]));
            MakeMove(new Move(from, fromRow, to, Tableau[to].Count));
        }

        private void MakeSingleMove(Move move)
        {
            // Record the move.
            if (TraceMoves)
            {
                Utils.WriteLine("Move {0}: {1}", Tableau.Moves.Count, move);
            }

            // Make the move.
            Tableau.Move(move);
        }
    }
}
