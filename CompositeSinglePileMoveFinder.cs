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
        private Tableau intermediateTableau;
        private HoldingStack holdingStack;

        private int from;
        private Pile fromPile;
        private OffloadInfo offload;
        private int best;
        private int order;
        private int emptyPilesLeft;
        private bool turnsOverCard;

        public CompositeSinglePileMoveFinder(Game game)
            : base(game)
        {
            used = new PileList();
            roots = new PileList();
            workingTableau = new Tableau();
            intermediateTableau = new Tableau();
            holdingStack = new HoldingStack();
        }

        public void Check(int column)
        {
            from = column;
            fromPile = Tableau[from];
            if (fromPile.Count == 0)
            {
                // No cards.
                return;
            }

            // Configure the data structures.
            workingTableau.Variation = Variation;
            intermediateTableau.Variation = Variation;

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
                Card uncoveredCard = Tableau[move.From][move.FromRow - 1];
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
            emptyPilesLeft = Tableau.NumberOfEmptyPiles;
            int runs = roots.Count - 1;
            offload = OffloadInfo.Empty;
            turnsOverCard = false;
            SupplementaryMoves.Clear();

            // Initialize the pile map.
            workingTableau.CopyUpPiles(Tableau);

            if (!uncoveringMove.IsEmpty)
            {
                // Update the map for the uncovering move but don't
                // include its order contribution so we don't make
                // the uncovering move unless it is really necessary.
                SupplementaryMoves.Add(uncoveringMove);
                workingTableau.Move(uncoveringMove);
            }

            // Check all the roots.
            int offloads = 0;
            for (int n = 1; n < roots.Count; n++)
            {
                int rootRow = roots[n];
                Card rootCard = fromPile[rootRow];
                int runLength = roots[n - 1] - roots[n];
                int suits = fromPile.CountSuits(rootRow, rootRow + runLength);
                int maxExtraSuits = ExtraSuits(emptyPilesLeft);
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
                    if (!card.IsEmpty && card.IsTargetFor(rootCard))
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
                            // Not enough empty piles to invert.
                            return;
                        }

                        // Update the state.
                        offload.Suits += suits - (suitsMatch ? 1 : 0);
                    }

                    // Try to move this run.
                    if (suits - 1 > maxExtraSuits)
                    {
                        // Try using holding piles.
                        suits -= FindHolding(workingTableau, holdingStack, false, fromPile, from, rootRow, rootRow + runLength, to, maxExtraSuits);
                        if (suits - 1 > maxExtraSuits)
                        {
                            // Not enough empty piles.
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
                            AddMove(workingTableau, MoveFlags.Empty, order);
                        }
                        return;
                    }

                    // Check for partial offload.
                    if (offloads > 0)
                    {
                        AddMove(workingTableau, MoveFlags.Empty, order);
                    }

                    // Try to offload this run.
                    if (emptyPilesLeft == 0)
                    {
                        // Not enough empty piles.
                        return;
                    }
                    to = Tableau.EmptyPiles[0];
                    int maxExtraSuitsOnePile = ExtraSuits(emptyPilesLeft - 1) + 1;
                    if (suits > maxExtraSuitsOnePile)
                    {
                        // Try using holding piles.
                        suits -= FindHolding(workingTableau, holdingStack, false, fromPile, from, rootRow, rootRow + runLength, to, maxExtraSuits);
                        if (suits > maxExtraSuits)
                        {
                            // Still not enough empty piles.
                            return;
                        }
                    }
                    int emptyPilesUsed = EmptyPilesUsed(emptyPilesLeft, suits);
                    emptyPilesLeft -= emptyPilesUsed;
                    offload = new OffloadInfo(n, to, suits, emptyPilesUsed, workingTableau[to]);
                    type = offload.SinglePile ? MoveType.Basic : MoveType.Unload;
                    isOffload = true;
                    offloads++;
                }

                // Do the move and the holding moves.
                HoldingSet holdingSet = holdingStack.Set;
                bool undoHolding = !isOffload;
                AddSupplementaryMove(workingTableau, new Move(type, from, rootRow, to), fromPile, holdingSet, undoHolding);

                if (rootRow == 0 && Tableau.GetDownCount(from) == 0)
                {
                    // Got to the bottom of the pile
                    // and created an empty pile.
                    emptyPilesLeft++;
                }

                // Check whether the offload matches the new from or to piles.
                if (!isOffload)
                {
                    CheckOffload(rootRow, to);
                }

                // Check whether any of the one run piles now match
                // the new from or to piles.
                for (int i = 0; i < OneRunPiles.Count; i++)
                {
                    if (CheckOneRun(rootRow, to, OneRunPiles[i]))
                    {
                        // Found an emptying move.
                        return;
                    }
                }
            }

            // Check for unload that needs to be reloaded.
            if (!offload.IsEmpty && !offload.SinglePile)
            {
                if (Tableau.GetDownCount(from) != 0)
                {
                    // Can't reload.
                    return;
                }
                else
                {
                    // Reload the offload onto the now empty pile.
                    SupplementaryMoves.Add(new Move(MoveType.Reload, offload.To, 0, from, 0));
                }
            }

            // Determine move type.
            int downCount = Tableau.GetDownCount(from);
            MoveFlags flags = MoveFlags.Empty;
            if (downCount != 0 || turnsOverCard)
            {
                flags |= MoveFlags.TurnsOverCard;
            }
            if (offload.IsEmpty && downCount == 0)
            {
                flags |= MoveFlags.CreatesEmptyPile;
            }
            if (!offload.IsEmpty && downCount != 0)
            {
                flags |= MoveFlags.UsesEmptyPile;
            }
            AddMove(workingTableau, flags, order);
        }

        private void CheckOffload(int rootRow, int to)
        {
            if (offload.IsEmpty)
            {
                // No offload to check.
                return;
            }

            int offloadRootRow = roots[offload.Root];
            Card offloadRootCard = fromPile[offloadRootRow];
            int offloadSuits = offload.Suits;
            int offloadMaxExtraSuits = ExtraSuits(emptyPilesLeft);
            MoveType offloadType = offload.SinglePile ? MoveType.Basic : MoveType.Reload;

            // Check whether offload matches from pile.
            if (rootRow > 0 && offloadRootCard.IsSourceFor(fromPile[rootRow - 1]))
            {
                // Check whether we can make the move.
                bool canMove = true;
                holdingStack.Clear();
                if (offload.SinglePile && offloadSuits - 1 > offloadMaxExtraSuits)
                {
                    // Not enough empty piles.
                    offloadSuits -= FindHolding(workingTableau, holdingStack, false, offload.Pile, offload.To, 0, offload.Pile.Count, from, offloadMaxExtraSuits);
                    if (offloadSuits - 1 > offloadMaxExtraSuits)
                    {
                        // Not enough empty piles and/or holding piles.
                        canMove = false;
                    }
                }

                if (canMove)
                {
                    // Prepare the intermediate tableau.
                    intermediateTableau.CopyUpPiles(workingTableau);

                    // Offload matches from pile.
                    int count = AddSupplementaryMove(intermediateTableau, new Move(offloadType, offload.To, 0, from), offload.Pile, holdingStack.Set, true);

                    // Add the intermediate move.
                    AddMove(intermediateTableau, MoveFlags.Empty, order + GetOrder(fromPile[rootRow - 1], offloadRootCard));

                    // Restore supplementary moves.
                    RestoreSupplementaryMoves(count);
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
                    // Not enough empty piles.
                    offloadSuits -= FindHolding(workingTableau, holdingStack, false, offload.Pile, offload.To, 0, offload.Pile.Count, to, offloadMaxExtraSuits);
                    if (offloadSuits - 1 > offloadMaxExtraSuits)
                    {
                        // Not enough empty piles and/or holding piles.
                        canMove = false;
                    }
                }

                if (canMove)
                {
                    // Record the order improvement.
                    order += GetOrder(toCard, offloadRootCard);

                    // Found a home for the offload.
                    AddSupplementaryMove(workingTableau, new Move(offloadType, offload.To, 0, to), offload.Pile, holdingStack.Set, true);

                    // Update the state.
                    emptyPilesLeft += offload.EmptyPilesUsed;
                    offload = OffloadInfo.Empty;
                }
            }
        }

        private void RestoreSupplementaryMoves(int count)
        {
            // Restore state of supplementary moves prior to intermediate move.
            for (int i = 0; i < count; i++)
            {
                SupplementaryMoves.RemoveAt(SupplementaryMoves.Count - 1);
            }
        }

        private bool CheckOneRun(int rootRow, int to, int oneRun)
        {
            Pile oneRunPile = workingTableau[oneRun];

            // Check whether the one run pile matches from pile.
            if (rootRow > 0 && oneRunPile.Count != 0 && oneRunPile[0].IsSourceFor(fromPile[rootRow - 1]))
            {
                if (TryToAddOneRunMove(rootRow, oneRun, from, fromPile[rootRow - 1]))
                {
                    return true;
                }
            }

            // Check whether the one run pile matches to pile.
            Card toCard = workingTableau.GetCard(to);
            if (oneRunPile.Count != 0 && oneRunPile[0].IsSourceFor(toCard))
            {
                if (TryToAddOneRunMove(rootRow, oneRun, to, toCard))
                {
                    return true;
                }
            }

            // Couldn't find an emptying move.
            return false;
        }

        private bool TryToAddOneRunMove(int rootRow, int oneRun, int target, Card targetCard)
        {
            Pile oneRunPile = workingTableau[oneRun];
            Card oneRunRootCard = oneRunPile[0];

            // Check whether we can make the move.
            int oneRunSuits = oneRunPile.CountSuits();
            int oneRunMaxExtraSuits = ExtraSuits(emptyPilesLeft);
            holdingStack.Clear();
            if (oneRunSuits - 1 > oneRunMaxExtraSuits)
            {
                oneRunSuits -= FindHolding(workingTableau, holdingStack, false, oneRunPile, oneRun, 0, oneRunPile.Count, target, oneRunMaxExtraSuits);
                if (oneRunSuits - 1 > oneRunMaxExtraSuits)
                {
                    // Not enough empty piles and/or holding piles.
                    return false;
                }
            }

            // Prepare the intermediate tableau.
            intermediateTableau.CopyUpPiles(workingTableau);

            // Found a home for the one run pile.
            int count = AddSupplementaryMove(intermediateTableau, new Move(oneRun, 0, target), oneRunPile, holdingStack.Set, true);

            // If we've already moved the whole from pile
            // then we've inherited some flags.
            MoveFlags baseFlags = MoveFlags.Empty;
#if true
            if (rootRow == 0)
            {
                baseFlags = Tableau.GetDownCount(from) == 0 ? MoveFlags.CreatesEmptyPile : MoveFlags.TurnsOverCard;
            }
#endif

            // Handle the messy cases.
            if (offload.IsEmpty)
            {
                if (Tableau.GetDownCount(oneRun) == 0)
                {
                    // Add the emptying move.
                    AddMove(intermediateTableau, baseFlags | MoveFlags.CreatesEmptyPile, order + GetOrder(targetCard, oneRunRootCard));
                    return true;
                }

                // Add the intermediate move.
                AddMove(intermediateTableau, baseFlags | MoveFlags.TurnsOverCard, order + GetOrder(targetCard, oneRunRootCard));
                if (baseFlags.CreatesEmptyPile())
                {
                    return true;
                }

                // Restore supplementary moves.
                RestoreSupplementaryMoves(count);
                return false;
            }

            if (offload.SinglePile && Tableau.GetDownCount(oneRun) == 0)
            {
                // Add the intermediate empty pile preserving move.
                AddMove(intermediateTableau, baseFlags, order + GetOrder(targetCard, oneRunRootCard));
                if (baseFlags.CreatesEmptyPile())
                {
                    return true;
                }

                // Restore supplementary moves.
                RestoreSupplementaryMoves(count);
                return false;
            }

#if true
            // Restore supplementary moves.
            RestoreSupplementaryMoves(count);
            return false;
#else
            if (target == from)
            {
                // We can't find a satisfactory ending for this move
                // unless the offload can be reloaded onto the
                // from pile.

                // Restore supplementary moves.
                RestoreSupplementaryMoves(count);
                return false;
            }

            // Update the map.
            map.Move(oneRun, 0, target);

            // This move now turns over a card no matter what.
            turnsOverCard = true;
            return false;
#endif
        }

        private int AddSupplementaryMove(Tableau tableau, Move move, Pile pile, HoldingSet holdingSet, bool undoHolding)
        {
            int count = 0;

            // Add moves to the holding piles.
            foreach (HoldingInfo holding in holdingSet.Forwards)
            {
                tableau.Move(holding.From, holding.FromRow, holding.To);
                SupplementaryMoves.Add(new Move(MoveType.Basic, MoveFlags.Holding, move.From, -holding.Length, holding.To));
                count++;
            }

            // Add the primary move.
            tableau.Move(move.From, move.FromRow, move.To);
            SupplementaryMoves.Add(move);
            count++;

            if (undoHolding)
            {
                // Undo moves from the holding piles.
                foreach (HoldingInfo holding in holdingSet.Backwards)
                {
                    if (!tableau.TryToMove(holding.To, -holding.Length, move.To))
                    {
                        break;
                    }
                    SupplementaryMoves.Add(new Move(MoveType.Basic, MoveFlags.UndoHolding, holding.To, -holding.Length, move.To));
                    count++;
                }
            }

            return count;
        }

        private void AddMove(Tableau tableau, MoveFlags flags, int totalOrder)
        {
#if false
            // Check which move is better.
            if (best != -1)
            {
                Move previous = Candidates[best];
                int previousChangeInEmptyPiles = previous.Flags.ChangeInEmptyPiles();
                int currentChangeInEmptyPiles = flags.ChangeInEmptyPiles();
                if (previousChangeInEmptyPiles >= currentChangeInEmptyPiles)
                {
                    if (previousChangeInEmptyPiles > currentChangeInEmptyPiles)
                    {
                        return;
                    }
                    int previousTurnsOverCard = previous.Flags.TurnsOverCard() ? 1 : 0;
                    int currentTurnsOverCard = flags.TurnsOverCard() ? 1 : 0;
                    if (previousTurnsOverCard >= currentTurnsOverCard)
                    {
                        if (previousTurnsOverCard > currentTurnsOverCard)
                        {
                            return;
                        }
                        int previousOrder = previous.ToRow;
                        int currentOrder = totalOrder;
                        if (previousOrder >= currentOrder)
                        {
                            return;
                        }
                    }
                }

                // Clear out the previous best move.
                Candidates[best] = Move.Empty;
            }
#endif

            // Add the scoring move and the accumulated supplementary moves.
            best = Candidates.Count;
            MoveFlags discardFlag = tableau.DiscardPiles.Count != 0 ? MoveFlags.Discards : MoveFlags.Empty;
            Candidates.Add(new Move(MoveType.CompositeSinglePile, flags | discardFlag, from, 0, 0, totalOrder, -1, AddSupplementary()));
        }
    }
}
