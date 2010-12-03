using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Spider.Collections;
using Spider.Engine;

namespace Spider.GamePlay
{
    public class CompositeSinglePileMoveFinder : GameAdapter
    {
        private int from;
        private Pile fromPile;
        private int best;
        private int order;
        private int tableauCheckPoint;
        private int supplementaryMovesCount;

        public CompositeSinglePileMoveFinder(Game game)
            : base(game)
        {
            UncoveringMoves = new MoveList();
            OneRunPiles = new PileList();
            Used = new PileList();
            Roots = new PileList();
            WorkingTableau = new Tableau();
            HoldingStack = new HoldingStack();
            SupplementaryMoves = new MoveList();
            MoveStack = new FastList<Move>();
        }

        private MoveList UncoveringMoves { get; set; }
        private PileList OneRunPiles { get; set; }
        private PileList Used { get; set; }
        private PileList Roots { get; set; }
        private Tableau WorkingTableau { get; set; }
        private HoldingStack HoldingStack { get; set; }
        private OffloadInfo Offload { get; set; }
        private MoveList SupplementaryMoves { get; set; }
        private FastList<Move> MoveStack { get; set; }

        public void Find()
        {
            int maxExtraSuits = ExtraSuits(FindTableau.NumberOfSpaces);

            FindUncoveringMoves(maxExtraSuits);
            FindOneRunPiles();

            for (int from = 0; from < Tableau.NumberOfPiles; from++)
            {
                Check(from);
            }
        }

        private void FindUncoveringMoves(int maxExtraSuits)
        {
            // Find all uncovering moves.
            UncoveringMoves.Clear();
            for (int from = 0; from < NumberOfPiles; from++)
            {
                Pile fromPile = FindTableau[from];
                int fromRow = fromPile.Count - RunFinder.GetRunUpAnySuit(from);
                if (fromRow == 0)
                {
                    continue;
                }
                int fromSuits = RunFinder.CountSuits(from, fromRow);
                Card fromCard = fromPile[fromRow];
                PileList faceList = FaceLists[(int)fromCard.Face + 1];
                for (int i = 0; i < faceList.Count; i++)
                {
                    HoldingStack.Clear();
                    int to = faceList[i];
                    if (fromSuits - 1 > maxExtraSuits)
                    {
                        int holdingSuits = FindHolding(FindTableau, HoldingStack, false, fromPile, from, fromRow, fromPile.Count, to, maxExtraSuits);
                        if (fromSuits - 1 > maxExtraSuits + holdingSuits)
                        {
                            break;
                        }
                    }
                    Pile toPile = FindTableau[to];
                    Card toCard = toPile[toPile.Count - 1];
                    int order = GetOrder(toCard, fromCard);
                    UncoveringMoves.Add(new Move(from, fromRow, to, order, AddHolding(HoldingStack.Set)));
                }
            }
        }

        private void FindOneRunPiles()
        {
            OneRunPiles.Clear();
            for (int column = 0; column < NumberOfPiles; column++)
            {
                int upCount = FindTableau[column].Count;
                if (upCount != 0 && upCount == RunFinder.GetRunUpAnySuit(column))
                {
                    OneRunPiles.Add(column);
                }
            }
        }

        private void Check(int column)
        {
            from = column;
            fromPile = FindTableau[from];
            if (fromPile.Count == 0)
            {
                // No cards.
                return;
            }

            // Configure the working tableau.
            WorkingTableau.Variation = Variation;

            // Find roots.
            Roots.Clear();
            int row = fromPile.Count;
            Roots.Add(row);
            while (row > 0)
            {
                int count = fromPile.GetRunUpAnySuit(row);
                row -= count;
                Roots.Add(row);
            }
            int runs = Roots.Count - 1;
            if (runs <= 1)
            {
                // Not at least two runs.
                return;
            }

            // Check first with no uncovering moves.
            best = -1;
            CheckOne(Move.Empty);

            Used.Clear();
            for (int i = 0; i < UncoveringMoves.Count; i++)
            {
                // Make sure the move doesn't interfere.
                Move move = UncoveringMoves[i];
                if (move.From == from || move.To == from)
                {
                    continue;
                }

                // Only need to try an uncovered card once.
                if (Used.Contains(move.From))
                {
                    continue;
                }
                Used.Add(move.From);

                // The uncovered card has to match a root to be useful.
                Card uncoveredCard = FindTableau[move.From][move.FromRow - 1];
                bool matchesRoot = false;
                for (int j = 1; j < Roots.Count; j++)
                {
                    if (uncoveredCard.IsTargetFor(fromPile[Roots[j]]))
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
            int runs = Roots.Count - 1;
            Offload = OffloadInfo.Empty;
            SupplementaryMoves.Clear();

            // Initialize the pile map.
            WorkingTableau.Clear();
            WorkingTableau.CopyUpPiles(FindTableau);
            WorkingTableau.BlockDownPiles(FindTableau);

            if (!uncoveringMove.IsEmpty)
            {
                // Update the map for the uncovering move but don't
                // include its order contribution so we don't make
                // the uncovering move unless it is really necessary.
                MoveStack.Clear();
                for (int next = uncoveringMove.HoldingNext; next != -1; next = SupplementaryList[next].Next)
                {
                    Move holdingMove = SupplementaryList[next];
                    SupplementaryMoves.Add(new Move(MoveType.Basic, MoveFlags.Holding, uncoveringMove.From, holdingMove.FromRow, holdingMove.To));
                    WorkingTableau.Move(holdingMove);
                    MoveStack.Push(new Move(MoveType.Basic, MoveFlags.UndoHolding, holdingMove.To, -holdingMove.ToRow, uncoveringMove.To));
                }
                SupplementaryMoves.Add(uncoveringMove);
                WorkingTableau.Move(uncoveringMove);
                while (MoveStack.Count > 0)
                {
                    Move holdingMove = MoveStack.Pop();
                    if (!WorkingTableau.IsValid(holdingMove))
                    {
                        break;
                    }
                    SupplementaryMoves.Add(holdingMove);
                    WorkingTableau.Move(holdingMove);
                }
            }

            // Check all the roots.
            int offloads = 0;
            for (int n = 1; n < Roots.Count; n++)
            {
                int rootRow = Roots[n];
                Card rootCard = fromPile[rootRow];
                int runLength = Roots[n - 1] - Roots[n];
                int suits = fromPile.CountSuits(rootRow, rootRow + runLength);
                int maxExtraSuits = ExtraSuits(GetNumberOfSpacesLeft());
                bool suitsMatch = false;
                HoldingStack.Clear();

                // Try to find the best matching target.
                int to = -1;
                for (int i = 0; i < NumberOfPiles; i++)
                {
                    if (i == from)
                    {
                        continue;
                    }
                    Card card = WorkingTableau.GetCard(i);
                    if (card.IsTargetFor(rootCard))
                    {
                        if (!Offload.IsEmpty && to == Offload.To)
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
                    if (!Offload.IsEmpty && to == Offload.To)
                    {
                        if (!Offload.SinglePile)
                        {
                            // Not enough spaces to invert.
                            return;
                        }
                    }

                    // Try to move this run.
                    if (suits - 1 > maxExtraSuits)
                    {
                        // Try using holding piles.
                        suits -= FindHolding(WorkingTableau, HoldingStack, false, fromPile, from, rootRow, rootRow + runLength, to, maxExtraSuits);
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
                    if (!Offload.IsEmpty)
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
                    to = WorkingTableau.Spaces[0];
                    int maxExtraSuitsOnePile = ExtraSuits(GetNumberOfSpacesLeft() - 1) + 1;
                    if (suits > maxExtraSuitsOnePile)
                    {
                        // Try using holding piles.
                        suits -= FindHolding(WorkingTableau, HoldingStack, false, fromPile, from, rootRow, rootRow + runLength, to, maxExtraSuits);
                        if (suits > maxExtraSuitsOnePile)
                        {
                            // Still not enough spaces.
                            return;
                        }
                    }
                    int numberOfSpacesUsed = SpacesUsed(GetNumberOfSpacesLeft(), suits);
                    Offload = new OffloadInfo(to, numberOfSpacesUsed);
                    type = Offload.SinglePile ? MoveType.Basic : MoveType.Unload;
                    isOffload = true;
                    offloads++;
                }

                // Do the move and the holding moves.
                HoldingSet holdingSet = HoldingStack.Set;
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
            if (!Offload.IsEmpty && !Offload.SinglePile)
            {
                if (FindTableau.GetDownCount(from) != 0)
                {
                    // Can't reload.
                    return;
                }
                else
                {
                    // Reload the offload onto the now empty space.
                    SupplementaryMoves.Add(new Move(MoveType.Reload, Offload.To, 0, from, 0));
                }
            }

            // Add the move.
            AddMove(order);
        }

        private int GetNumberOfSpacesLeft()
        {
            int n = WorkingTableau.NumberOfSpaces;
            if (!Offload.IsEmpty)
            {
                n -= Offload.NumberOfSpacesUsed - 1;
            }
            return n;
        }

        private void CheckOffload(int to)
        {
            if (Offload.IsEmpty)
            {
                // No offload to check.
                return;
            }

            Pile offloadPile = WorkingTableau[Offload.To];
            if (offloadPile.Count == 0)
            {
                // A discard emptied the offload pile.
                Offload = OffloadInfo.Empty;
                return;
            }

            Card offloadRootCard = offloadPile[0];
            int offloadSuits = offloadPile.CountSuits();
            int offloadMaxExtraSuits = ExtraSuits(GetNumberOfSpacesLeft());
            MoveType offloadType = Offload.SinglePile ? MoveType.Basic : MoveType.Reload;

            // Check whether offload matches from pile.
            Card fromCard = WorkingTableau.GetCard(from);
            if (offloadRootCard.IsSourceFor(fromCard))
            {
                // Check whether we can make the move.
                bool canMove = true;
                HoldingStack.Clear();
                if (Offload.SinglePile && offloadSuits - 1 > offloadMaxExtraSuits)
                {
                    // Not enough spaces.
                    offloadSuits -= FindHolding(WorkingTableau, HoldingStack, false, offloadPile, Offload.To, 0, offloadPile.Count, from, offloadMaxExtraSuits);
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
                    AddSupplementaryMove(new Move(offloadType, Offload.To, 0, from), offloadPile, HoldingStack.Set, true);

                    // Add the intermediate move.
                    AddMove(order + GetOrder(fromCard, offloadRootCard));

                    // Restore working state.
                    RestoreWorkingState();
                }
            }

            // Check whether offoad matches to pile.
            Card toCard = WorkingTableau.GetCard(to);
            if (offloadRootCard.IsSourceFor(toCard))
            {

                // Check whether we can make the move.
                bool canMove = true;
                HoldingStack.Clear();
                if (Offload.SinglePile && offloadSuits - 1 > offloadMaxExtraSuits)
                {
                    // Not enough spaces.
                    offloadSuits -= FindHolding(WorkingTableau, HoldingStack, false, offloadPile, Offload.To, 0, offloadPile.Count, to, offloadMaxExtraSuits);
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
                    AddSupplementaryMove(new Move(offloadType, Offload.To, 0, to), offloadPile, HoldingStack.Set, true);

                    // Update the state.
                    Offload = OffloadInfo.Empty;
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
            Pile oneRunPile = WorkingTableau[oneRun];
            if (oneRunPile.Count == 0)
            {
                return false;
            }
            Card oneRunRootCard = oneRunPile[0];

            // Check whether the one run pile matches the target pile.
            Card targetCard = WorkingTableau.GetCard(target);
            if (!oneRunRootCard.IsSourceFor(targetCard))
            {
                return false;
            }

            // Check whether we can make the move.
            int oneRunSuits = oneRunPile.CountSuits();
            int oneRunMaxExtraSuits = ExtraSuits(GetNumberOfSpacesLeft());
            HoldingStack.Clear();
            if (oneRunSuits - 1 > oneRunMaxExtraSuits)
            {
                oneRunSuits -= FindHolding(WorkingTableau, HoldingStack, false, oneRunPile, oneRun, 0, oneRunPile.Count, target, oneRunMaxExtraSuits);
                if (oneRunSuits - 1 > oneRunMaxExtraSuits)
                {
                    // Not enough spaces and/or holding piles.
                    return false;
                }
            }

            // Handle the messy cases.
            if (Offload.IsEmpty || Offload.SinglePile && FindTableau.GetDownCount(oneRun) == 0)
            {
                // Save working state.
                SaveWorkingState();

                // Found a home for the one run pile.
                AddSupplementaryMove(new Move(oneRun, 0, target), oneRunPile, HoldingStack.Set, true);

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
            for (int i = 0; i < holdingSet.Count; i++)
            {
                HoldingInfo holding = holdingSet[i];
                Move holdingMove = new Move(MoveType.Basic, MoveFlags.Holding, move.From, -holding.Length, holding.To);
                WorkingTableau.Move(holdingMove);
                SupplementaryMoves.Add(holdingMove);
            }

            // Add the primary move.
            WorkingTableau.Move(new Move(move.From, move.FromRow, move.To));
            SupplementaryMoves.Add(move);

            if (undoHolding)
            {
                // Undo moves from the holding piles.
                for (int i = holdingSet.Count - 1; i >= 0; i--)
                {
                    HoldingInfo holding = holdingSet[i];
                    Move holdingMove = new Move(MoveType.Basic, MoveFlags.UndoHolding, holding.To, -holding.Length, move.To);
                    if (!WorkingTableau.TryToMove(holdingMove))
                    {
                        break;
                    }
                    SupplementaryMoves.Add(holdingMove);
                }
            }
        }

        private void SaveWorkingState()
        {
            tableauCheckPoint = WorkingTableau.CheckPoint;
            supplementaryMovesCount = SupplementaryMoves.Count;
        }

        private void RestoreWorkingState()
        {
            // Restore state of the working tableau prior to intermediate move.
            WorkingTableau.Revert(tableauCheckPoint);

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
            if (WorkingTableau.DiscardPiles.Count != 0)
            {
                flags |= MoveFlags.Discards;
            }
            if (WorkingTableau.NumberOfSpaces > FindTableau.NumberOfSpaces)
            {
                flags |= MoveFlags.CreatesSpace;
            }
            if (WorkingTableau.NumberOfSpaces < FindTableau.NumberOfSpaces)
            {
                flags |= MoveFlags.UsesSpace;
            }
            for (int column = 0; column < WorkingTableau.NumberOfPiles; column++)
            {
                Pile pile = WorkingTableau[column];
                if (pile.Count == 1 && pile[0].IsEmpty)
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
            Algorithm.ProcessCandidate(new Move(MoveType.CompositeSinglePile, flags, from, 0, 0, totalOrder, -1, AddSupplementary(SupplementaryMoves)));

            return flags.CreatesSpace();
        }
    }
}
