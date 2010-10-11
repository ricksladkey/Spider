using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    public class CompositeSinglePileMoveFinder : GameHelper
    {
        private PileList used;
        private PileList roots;
        private Tableau workingTableau;
        private HoldingStack holdingStack;

        private int from;
        private Pile fromPile;
        private OffloadInfo offload;
        private int best;
        private int order;

        private int tableauTimeStamp;
        private int supplementaryMovesCount;

        public CompositeSinglePileMoveFinder(Game game)
            : base(game)
        {
            used = new PileList();
            roots = new PileList();
            workingTableau = new Tableau();
            holdingStack = new HoldingStack();
        }

        public void Check(int column)
        {
            from = column;
            fromPile = FindTableau[from];
            if (fromPile.Count == 0)
            {
                // No cards.
                return;
            }

            // Configure the working tableau.
            workingTableau.Variation = Variation;

            // Find roots.
            roots.Clear();
            int row = fromPile.Count;
            roots.Add(row);
            while (row > 0)
            {
                int count = fromPile.GetRunUpAnySuit(row);
                row -= count;
                roots.Add(row);
            }
            int runs = roots.Count - 1;
            if (runs <= 1)
            {
                // Not at least two runs.
                return;
            }

            // Check first with no uncovering moves.
            best = -1;
            CheckOne(Move.Empty);

            used.Clear();
            for (int i = 0; i < UncoveringMoves.Count; i++)
            {
                // Make sure the move doesn't interfere.
                Move move = UncoveringMoves[i];
                if (move.From == from || move.To == from)
                {
                    continue;
                }

                // Only need to try an uncovered card once.
                if (used.Contains(move.From))
                {
                    continue;
                }
                used.Add(move.From);

                // The uncovered card has to match a root to be useful.
                Card uncoveredCard = FindTableau[move.From][move.FromRow - 1];
                bool matchesRoot = false;
                for (int j = 1; j < roots.Count; j++)
                {
                    if (uncoveredCard.IsTargetFor(fromPile[roots[j]]))
                    {
                        matchesRoot = true;
                        break;
                    }
                }
                if (!matchesRoot)
                {
                    continue;
                }

                // Try again to find a composite single pile move.
                move.ToRow = -1;
                CheckOne(move);
            }
        }

        private void CheckOne(Move uncoveringMove)
        {
            // Prepare data structures.
            order = 0;
            int runs = roots.Count - 1;
            offload = OffloadInfo.Empty;
            SupplementaryMoves.Clear();

            // Initialize the pile map.
            workingTableau.ClearAll();
            workingTableau.CopyUpPiles(FindTableau);
            workingTableau.BlockDownPiles(FindTableau);

            if (!uncoveringMove.IsEmpty)
            {
                // Update the map for the uncovering move but don't
                // include its order contribution so we don't make
                // the uncovering move unless it is really necessary.
                Stack<Move> moveStack = new Stack<Move>();
                for (int next = uncoveringMove.HoldingNext; next != -1; next = SupplementaryList[next].Next)
                {
                    Move holdingMove = SupplementaryList[next];
                    SupplementaryMoves.Add(new Move(MoveType.Basic, MoveFlags.Holding, uncoveringMove.From, holdingMove.FromRow, holdingMove.To));
                    workingTableau.Move(holdingMove);
                    moveStack.Push(new Move(MoveType.Basic, MoveFlags.UndoHolding, holdingMove.To, -holdingMove.ToRow, uncoveringMove.To));
                }
                SupplementaryMoves.Add(uncoveringMove);
                workingTableau.Move(uncoveringMove);
                while (moveStack.Count > 0)
                {
                    Move holdingMove = moveStack.Pop();
                    if (!workingTableau.MoveIsValid(holdingMove))
                    {
                        break;
                    }
                    SupplementaryMoves.Add(holdingMove);
                    workingTableau.Move(holdingMove);
                }
            }

            // Check all the roots.
            int offloads = 0;
            for (int n = 1; n < roots.Count; n++)
            {
                int rootRow = roots[n];
                Card rootCard = fromPile[rootRow];
                int runLength = roots[n - 1] - roots[n];
                int suits = fromPile.CountSuits(rootRow, rootRow + runLength);
                int maxExtraSuits = ExtraSuits(GetNumberOfSpacesLeft());
                bool suitsMatch = false;
                holdingStack.Clear();

                // Try to find the best matching target.
                int to = -1;
                for (int i = 0; i < NumberOfPiles; i++)
                {
                    if (i == from)
                    {
                        continue;
                    }
                    Card card = workingTableau.GetCard(i);
                    if (card.IsTargetFor(rootCard))
                    {
                        if (!offload.IsEmpty && to == offload.To)
                        {
                            to = -1;
                            suitsMatch = false;
                        }
                        if (!suitsMatch && card.Suit == rootCard.Suit)
                        {
                            to = i;
                            suitsMatch = true;
                        }
                        else if (to == -1)
                        {
                            to = i;
                        }
                    }
                }

                MoveType type = MoveType.Basic;
                bool isOffload = false;
                if (to != -1)
                {
                    // Check for inverting.
                    if (!offload.IsEmpty && to == offload.To)
                    {
                        if (!offload.SinglePile)
                        {
                            // Not enough spaces to invert.
                            return;
                        }
                    }

                    // Try to move this run.
                    if (suits - 1 > maxExtraSuits)
                    {
                        // Try using holding piles.
                        suits -= FindHolding(workingTableau, holdingStack, false, fromPile, from, rootRow, rootRow + runLength, to, maxExtraSuits);
                        if (suits - 1 > maxExtraSuits)
                        {
                            // Not enough spaces.
                            return;
                        }
                    }

                    // Record the order improvement.
                    order += GetOrder(true, suitsMatch);
                }
                else
                {
                    if (!offload.IsEmpty)
                    {
                        // Already have an offload.
                        return;
                    }

                    // It doesn't make sense to offload the last root.
                    if (rootRow == 0)
                    {
                        if (runs - 1 >= 2)
                        {
                            AddMove(order);
                        }
                        return;
                    }

                    // Check for partial offload.
                    if (offloads > 0)
                    {
                        AddMove(order);
                    }

                    // Try to offload this run.
                    if (GetNumberOfSpacesLeft() == 0)
                    {
                        // Not enough spaces.
                        return;
                    }
                    to = workingTableau.Spaces[0];
                    int maxExtraSuitsOnePile = ExtraSuits(GetNumberOfSpacesLeft() - 1) + 1;
                    if (suits > maxExtraSuitsOnePile)
                    {
                        // Try using holding piles.
                        suits -= FindHolding(workingTableau, holdingStack, false, fromPile, from, rootRow, rootRow + runLength, to, maxExtraSuits);
                        if (suits > maxExtraSuits)
                        {
                            // Still not enough spaces.
                            return;
                        }
                    }
                    int numberOfSpacesUsed = SpacesUsed(GetNumberOfSpacesLeft(), suits);
                    offload = new OffloadInfo(to, numberOfSpacesUsed);
                    type = offload.SinglePile ? MoveType.Basic : MoveType.Unload;
                    isOffload = true;
                    offloads++;
                }

                // Do the move and the holding moves.
                HoldingSet holdingSet = holdingStack.Set;
                bool undoHolding = !isOffload;
                AddSupplementaryMove(new Move(type, from, rootRow, to), fromPile, holdingSet, undoHolding);

                // Check whether the offload matches the new from or to piles.
                if (!isOffload)
                {
                    CheckOffload(to);
                }

                // Check whether any of the one run piles now match
                // the new from or to piles.
                for (int i = 0; i < OneRunPiles.Count; i++)
                {
                    if (CheckOneRun(to, OneRunPiles[i]))
                    {
                        // Found an emptying move.
                        return;
                    }
                }
            }

            // Check for unload that needs to be reloaded.
            if (!offload.IsEmpty && !offload.SinglePile)
            {
                if (FindTableau.GetDownCount(from) != 0)
                {
                    // Can't reload.
                    return;
                }
                else
                {
                    // Reload the offload onto the now empty space.
                    SupplementaryMoves.Add(new Move(MoveType.Reload, offload.To, 0, from, 0));
                }
            }

            // Add the move.
            AddMove(order);
        }

        private int GetNumberOfSpacesLeft()
        {
            int n = workingTableau.NumberOfSpaces;
            if (!offload.IsEmpty)
            {
                n -= offload.NumberOfSpacesUsed - 1;
            }
            return n;
        }

        private void CheckOffload(int to)
        {
            if (offload.IsEmpty)
            {
                // No offload to check.
                return;
            }

            Pile offloadPile = workingTableau[offload.To];
            if (offloadPile.Count == 0)
            {
                // A discard emptied the offload pile.
                offload = OffloadInfo.Empty;
                return;
            }

            Card offloadRootCard = offloadPile[0];
            int offloadSuits = offloadPile.CountSuits();
            int offloadMaxExtraSuits = ExtraSuits(GetNumberOfSpacesLeft());
            MoveType offloadType = offload.SinglePile ? MoveType.Basic : MoveType.Reload;

            // Check whether offload matches from pile.
            Card fromCard = workingTableau.GetCard(from);
            if (offloadRootCard.IsSourceFor(fromCard))
            {
                // Check whether we can make the move.
                bool canMove = true;
                holdingStack.Clear();
                if (offload.SinglePile && offloadSuits - 1 > offloadMaxExtraSuits)
                {
                    // Not enough spaces.
                    offloadSuits -= FindHolding(workingTableau, holdingStack, false, offloadPile, offload.To, 0, offloadPile.Count, from, offloadMaxExtraSuits);
                    if (offloadSuits - 1 > offloadMaxExtraSuits)
                    {
                        // Not enough spaces and/or holding piles.
                        canMove = false;
                    }
                }

                if (canMove)
                {
                    // Save working state.
                    SaveWorkingState();

                    // Offload matches from pile.
                    AddSupplementaryMove(new Move(offloadType, offload.To, 0, from), offloadPile, holdingStack.Set, true);

                    // Add the intermediate move.
                    AddMove(order + GetOrder(fromCard, offloadRootCard));

                    // Restore working state.
                    RestoreWorkingState();
                }
            }

            // Check whether offoad matches to pile.
            Card toCard = workingTableau.GetCard(to);
            if (offloadRootCard.IsSourceFor(toCard))
            {

                // Check whether we can make the move.
                bool canMove = true;
                holdingStack.Clear();
                if (offload.SinglePile && offloadSuits - 1 > offloadMaxExtraSuits)
                {
                    // Not enough spaces.
                    offloadSuits -= FindHolding(workingTableau, holdingStack, false, offloadPile, offload.To, 0, offloadPile.Count, to, offloadMaxExtraSuits);
                    if (offloadSuits - 1 > offloadMaxExtraSuits)
                    {
                        // Not enough spaces and/or holding piles.
                        canMove = false;
                    }
                }

                if (canMove)
                {
                    // Record the order improvement.
                    order += GetOrder(toCard, offloadRootCard);

                    // Found a home for the offload.
                    AddSupplementaryMove(new Move(offloadType, offload.To, 0, to), offloadPile, holdingStack.Set, true);

                    // Update the state.
                    offload = OffloadInfo.Empty;
                }
            }
        }

        private bool CheckOneRun(int to, int oneRun)
        {
            // Check whether the one run pile matches from pile.
            if (TryToAddOneRunMove(oneRun, from))
            {
                return true;
            }

            // Check whether the one run pile matches to pile.
            if (TryToAddOneRunMove(oneRun, to))
            {
                return true;
            }

            // Couldn't find an emptying move.
            return false;
        }

        private bool TryToAddOneRunMove(int oneRun, int target)
        {
            Pile oneRunPile = workingTableau[oneRun];
            if (oneRunPile.Count == 0)
            {
                return false;
            }
            Card oneRunRootCard = oneRunPile[0];

            // Check whether the one run pile matches the target pile.
            Card targetCard = workingTableau.GetCard(target);
            if (!oneRunRootCard.IsSourceFor(targetCard))
            {
                return false;
            }

            // Check whether we can make the move.
            int oneRunSuits = oneRunPile.CountSuits();
            int oneRunMaxExtraSuits = ExtraSuits(GetNumberOfSpacesLeft());
            holdingStack.Clear();
            if (oneRunSuits - 1 > oneRunMaxExtraSuits)
            {
                oneRunSuits -= FindHolding(workingTableau, holdingStack, false, oneRunPile, oneRun, 0, oneRunPile.Count, target, oneRunMaxExtraSuits);
                if (oneRunSuits - 1 > oneRunMaxExtraSuits)
                {
                    // Not enough spaces and/or holding piles.
                    return false;
                }
            }

            // Handle the messy cases.
            if (offload.IsEmpty || offload.SinglePile && FindTableau.GetDownCount(oneRun) == 0)
            {
                // Save working state.
                SaveWorkingState();

                // Found a home for the one run pile.
                AddSupplementaryMove(new Move(oneRun, 0, target), oneRunPile, holdingStack.Set, true);

                if (AddMove(order + GetOrder(targetCard, oneRunRootCard)))
                {
                    return true;
                }

                // Restore working state.
                RestoreWorkingState();
            }

            return false;
        }

        private void AddSupplementaryMove(Move move, Pile pile, HoldingSet holdingSet, bool undoHolding)
        {
            // Add moves to the holding piles.
            foreach (HoldingInfo holding in holdingSet.Forwards)
            {
                workingTableau.Move(holding.From, holding.FromRow, holding.To);
                SupplementaryMoves.Add(new Move(MoveType.Basic, MoveFlags.Holding, move.From, -holding.Length, holding.To));
            }

            // Add the primary move.
            workingTableau.Move(move.From, move.FromRow, move.To);
            SupplementaryMoves.Add(move);

            if (undoHolding)
            {
                // Undo moves from the holding piles.
                foreach (HoldingInfo holding in holdingSet.Backwards)
                {
                    if (!workingTableau.TryToMove(holding.To, -holding.Length, move.To))
                    {
                        break;
                    }
                    SupplementaryMoves.Add(new Move(MoveType.Basic, MoveFlags.UndoHolding, holding.To, -holding.Length, move.To));
                }
            }
        }

        private void SaveWorkingState()
        {
            tableauTimeStamp = workingTableau.TimeStamp;
            supplementaryMovesCount = SupplementaryMoves.Count;
        }

        private void RestoreWorkingState()
        {
            // Restore state of the working tableau prior to intermediate move.
            workingTableau.Revert(tableauTimeStamp);

            // Restore state of supplementary moves prior to intermediate move.
            while (SupplementaryMoves.Count > supplementaryMovesCount)
            {
                SupplementaryMoves.RemoveAt(SupplementaryMoves.Count - 1);
            }
        }

        private bool AddMove(int totalOrder)
        {
            // Infer move flags from state of the working tableau.
            MoveFlags flags = MoveFlags.Empty;
            if (workingTableau.DiscardPiles.Count != 0)
            {
                flags |= MoveFlags.Discards;
            }
            if (workingTableau.NumberOfSpaces > FindTableau.NumberOfSpaces)
            {
                flags |= MoveFlags.CreatesSpace;
            }
            if (workingTableau.NumberOfSpaces < FindTableau.NumberOfSpaces)
            {
                flags |= MoveFlags.UsesSpace;
            }
            for (int column = 0; column < workingTableau.NumberOfPiles; column++)
            {
                Pile pile = workingTableau[column];
                if (pile.Count == 1 && pile[0].IsUnknown)
                {
                    flags |= MoveFlags.TurnsOverCard;
                    break;
                }
            }

#if false
            // Check which move is better.
            if (best != -1)
            {
                Move previous = Candidates[best];
                int previousChangeInSpaces = previous.Flags.ChangeInSpaces();
                int currentChangeInSpaces = flags.ChangeInSpaces();
                if (previousChangeInSpaces >= currentChangeInSpaces)
                {
                    if (previousChangeInSpaces > currentChangeInSpaces)
                    {
                        return false;
                    }
                    int previousTurnsOverCard = previous.Flags.TurnsOverCard() ? 1 : 0;
                    int currentTurnsOverCard = flags.TurnsOverCard() ? 1 : 0;
                    if (previousTurnsOverCard >= currentTurnsOverCard)
                    {
                        if (previousTurnsOverCard > currentTurnsOverCard)
                        {
                            return false;
                        }
                        int previousOrder = previous.ToRow;
                        int currentOrder = totalOrder;
                        if (previousOrder >= currentOrder)
                        {
                            return false;
                        }
                    }
                }

                // Clear out the previous best move.
                Candidates[best] = Move.Empty;
            }
#endif

            // Add the scoring move and the accumulated supplementary moves.
            best = Candidates.Count;
            ProcessCandidate(new Move(MoveType.CompositeSinglePile, flags, from, 0, 0, totalOrder, -1, AddSupplementary()));

            return flags.CreatesSpace();
        }
    }
}
