using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    public class CompositeSinglePileMoveFinder : GameHelper
    {
        private Pile offloadPile;
        private PileList used;
        private PileList roots;
        private PileMap map;
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
            offloadPile = new Pile();
            used = new PileList();
            roots = new PileList();
            map = new PileMap();
            holdingStack = new HoldingStack();
        }

        public void Check(int column)
        {
            from = column;
            fromPile = UpPiles[from];
            if (fromPile.Count == 0)
            {
                // No cards.
                return;
            }

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
                Card uncoveredCard = UpPiles[move.From][move.FromRow - 1];
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
            emptyPilesLeft = EmptyPiles.Count;
            int runs = roots.Count - 1;
            offload = OffloadInfo.Empty;
            turnsOverCard = false;
            SupplementaryMoves.Clear();

            // Initialize the pile map.
            map.Update(UpPiles);

            if (!uncoveringMove.IsEmpty)
            {
                // Update the map for the uncovering move but don't
                // include its order contribution so we don't make
                // the uncovering move unless it is really necessary.
                map.Move(uncoveringMove);
                SupplementaryMoves.Add(uncoveringMove);
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
                    Card card = map.GetCard(i);
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
                        offload.Pile.AddRange(fromPile, rootRow, runLength);
                    }

                    // Try to move this run.
                    if (suits - 1 > maxExtraSuits)
                    {
                        // Try using holding piles.
                        suits -= FindHolding(map, holdingStack, false, fromPile, from, rootRow, rootRow + runLength, to, maxExtraSuits);
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
                    if (n == roots.Count - 1)
                    {
                        if (runs - 1 >= 2)
                        {
                            AddMove(MoveFlags.Empty, order);
                        }
                        return;
                    }

                    // Check for partial offload.
                    if (offloads > 0)
                    {
                        AddMove(MoveFlags.Empty, order);
                    }

                    // Try to offload this run.
                    if (emptyPilesLeft == 0)
                    {
                        // Not enough empty piles.
                        return;
                    }
                    to = EmptyPiles[0];
                    int maxExtraSuitsOnePile = ExtraSuits(emptyPilesLeft - 1) + 1;
                    if (suits > maxExtraSuitsOnePile)
                    {
                        // Try using holding piles.
                        suits -= FindHolding(map, holdingStack, false, fromPile, from, rootRow, rootRow + runLength, to, maxExtraSuits);
                        if (suits > maxExtraSuits)
                        {
                            // Still not enough empty piles.
                            return;
                        }
                    }
                    int emptyPilesUsed = EmptyPilesUsed(emptyPilesLeft, suits);
                    emptyPilesLeft -= emptyPilesUsed;
                    offload = new OffloadInfo(n, to, suits, emptyPilesUsed, offloadPile);
                    offload.Pile.Clear();
                    offload.Pile.AddRange(fromPile, rootRow, runLength - holdingStack.Set.Length);
                    type = offload.SinglePile ? MoveType.Basic : MoveType.Unload;
                    isOffload = true;
                    offloads++;
                }

                // Do the move and the holding moves.
                HoldingSet holdingSet = holdingStack.Set;
                bool undoHolding = !isOffload;
                AddSupplementaryMove(new Move(type, from, rootRow, to), fromPile, runLength, holdingSet, undoHolding);

                // Update the map.
                map.Move(from, rootRow, to);

                if (rootRow == 0 && DownPiles[from].Count == 0)
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
                if (DownPiles[from].Count != 0)
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
            int downCount = DownPiles[from].Count;
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
            AddMove(flags, order);
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
                    offloadSuits -= FindHolding(map, holdingStack, false, offload.Pile, offload.To, 0, offload.Pile.Count, from, offloadMaxExtraSuits);
                    if (offloadSuits - 1 > offloadMaxExtraSuits)
                    {
                        // Not enough empty piles and/or holding piles.
                        canMove = false;
                    }
                }

                if (canMove)
                {
                    // Offload matches from pile.
                    int count = AddSupplementaryMove(new Move(offloadType, offload.To, 0, from), offload.Pile, offload.Pile.Count, holdingStack.Set, true);

                    // Add the intermediate move.
                    AddMove(MoveFlags.Empty, order + GetOrder(fromPile[rootRow - 1], offloadRootCard));

                    // Restore supplementary moves.
                    RestoreSupplementaryMoves(count);
                }
            }

            // Check whether offoad matches to pile.
            Card toCard = map.GetCard(to);
            if (offloadRootCard.IsSourceFor(toCard))
            {

                // Check whether we can make the move.
                bool canMove = true;
                holdingStack.Clear();
                if (offload.SinglePile && offloadSuits - 1 > offloadMaxExtraSuits)
                {
                    // Not enough empty piles.
                    offloadSuits -= FindHolding(map, holdingStack, false, offload.Pile, offload.To, 0, offload.Pile.Count, to, offloadMaxExtraSuits);
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
                    AddSupplementaryMove(new Move(offloadType, offload.To, 0, to), offload.Pile, offload.Pile.Count, holdingStack.Set, true);

                    // Update the map.
                    map.Move(offload.To, 0, to);

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
            Card oneRunRootCard = map[oneRun][0];

            // Check whether one run pile matches from pile.
            if (rootRow > 0 && oneRunRootCard.IsSourceFor(fromPile[rootRow - 1]))
            {
                if (TryToAddOneRunMove(oneRun, from, fromPile[rootRow - 1]))
                {
                    return true;
                }
            }

            // Check whether one run pile matches to pile.
            Card toCard = map.GetCard(to);
            if (oneRunRootCard.IsSourceFor(toCard))
            {
                if (TryToAddOneRunMove(oneRun, to, toCard))
                {
                    return true;
                }
            }

            // Couldn't find an emptying move.
            return false;
        }

        private bool TryToAddOneRunMove(int oneRun, int target, Card targetCard)
        {
            Pile oneRunPile = map[oneRun];
            Card oneRunRootCard = oneRunPile[0];

            // Check whether we can make the move.
            int oneRunSuits = oneRunPile.CountSuits();
            int oneRunMaxExtraSuits = ExtraSuits(emptyPilesLeft);
            holdingStack.Clear();
            if (oneRunSuits - 1 > oneRunMaxExtraSuits)
            {
                oneRunSuits -= FindHolding(map, holdingStack, false, oneRunPile, oneRun, 0, oneRunPile.Count, target, oneRunMaxExtraSuits);
                if (oneRunSuits - 1 > oneRunMaxExtraSuits)
                {
                    // Not enough empty piles and/or holding piles.
                    return false;
                }
            }

            if (target == from && !offload.IsEmpty && DownPiles[oneRun].Count != 0)
            {
                // We can't find a satisfactory ending for this move
                // unless the offload can be reloaded onto the
                // from pile.
                return false;
            }

            // Found a home for the one run pile.
            int count = AddSupplementaryMove(new Move(oneRun, 0, target), oneRunPile, oneRunPile.Count, holdingStack.Set, true);

            if (offload.IsEmpty)
            {
                if (DownPiles[oneRun].Count == 0)
                {
                    // Add the emptying move.
                    AddMove(MoveFlags.CreatesEmptyPile, order + GetOrder(targetCard, oneRunRootCard));
                    return true;
                }

                // Add the intermediate move.
                AddMove(MoveFlags.TurnsOverCard, order + GetOrder(targetCard, oneRunRootCard));

                // Restore supplementary moves.
                RestoreSupplementaryMoves(count);
                return false;
            }

            if (offload.SinglePile && DownPiles[oneRun].Count == 0)
            {
                // Add the intermediate empty pile preserving move.
                AddMove(MoveFlags.Empty, order + GetOrder(targetCard, oneRunRootCard));

                // Restore supplementary moves.
                RestoreSupplementaryMoves(count);
                return false;
            }

            // Update the map.
            map.Move(oneRun, 0, target);

            // This move now turns over a card no matter what.
            turnsOverCard = true;
            return false;
        }

        private int AddSupplementaryMove(Move move, Pile pile, int runLength, HoldingSet holdingSet, bool undoHolding)
        {
            if (undoHolding)
            {
                // Add moves to the holding piles.
                foreach (HoldingInfo holding in holdingSet.Forwards)
                {
                    SupplementaryMoves.Add(new Move(MoveType.Basic, MoveFlags.Holding, move.From, -holding.Length, holding.To));
                }

                // Add the move.
                SupplementaryMoves.Add(move);

                // Undo moves from the holding piles.
                foreach (HoldingInfo holding in holdingSet.Backwards)
                {
                    SupplementaryMoves.Add(new Move(MoveType.Basic, MoveFlags.UndoHolding, holding.To, -holding.Length, move.To));
                }

                return 2 * holdingSet.Count + 1;
            }
            else
            {
                foreach (HoldingInfo holding in holdingSet.Forwards)
                {
                    // Add holding move and update the map.
                    SupplementaryMoves.Add(new Move(MoveType.Basic, MoveFlags.Holding, move.From, -holding.Length, holding.To));
                    map.Move(holding.From, holding.FromRow, holding.To);
                }

                // Add the move and update the map.
                SupplementaryMoves.Add(move);

                return holdingSet.Count + 1;
            }
        }

        private void AddMove(MoveFlags flags, int totalOrder)
        {
#if true
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
            Candidates.Add(new Move(MoveType.CompositeSinglePile, flags, from, 0, 0, totalOrder, -1, AddSupplementary()));
        }
    }
}
