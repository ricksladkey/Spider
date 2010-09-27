using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public class CompositeSinglePileMoveFinder
    {
        private Game game;

        private static int NumberOfPiles { get { return Game.NumberOfPiles; } }
        private PileMap UpPiles { get { return game.UpPiles; } }
        private PileMap DownPiles { get { return game.DownPiles; } }
        private MoveList UncoveringMoves { get { return game.UncoveringMoves; } }
        private MoveList SupplementaryMoves { get { return game.SupplementaryMoves; } }
        private MoveList Candidates { get { return game.Candidates; } }
        private PileList EmptyPiles { get { return game.EmptyPiles; } }

        private PileList used;
        private PileList roots;
        private CardMap map;
        private HoldingStack holdingStack;

        private Pile fromPile;
        private OffloadInfo offload;
        private int best;
        private int order;
        private int emptyPilesLeft;

        public CompositeSinglePileMoveFinder(Game game)
        {
            this.game = game;

            used = new PileList();
            roots = new PileList();
            map = new CardMap();
            holdingStack = new HoldingStack();
        }

        public void Check(int from)
        {
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
            order = 0;
            best = -1;
            CheckOne(from, roots, Move.Empty);

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
                    if (uncoveredCard.Face - 1 == fromPile[roots[j]].Face)
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
                order = move.ToRow;
                move.ToRow = -1;
                CheckOne(from, roots, move);
            }
        }

        private void CheckOne(int from, PileList roots, Move move)
        {
            // Prepare data structures.
            emptyPilesLeft = EmptyPiles.Count;
            int runs = roots.Count - 1;
            offload = OffloadInfo.Empty;
            SupplementaryMoves.Clear();

            // Initialize the pile map.
            map.Update(UpPiles);
            map[from] = Card.Empty;

            if (!move.IsEmpty)
            {
                map[move.To] = map[move.From];
                map[move.From] = UpPiles[move.From][move.FromRow - 1];
                SupplementaryMoves.Add(move);
            }

            // Check all the roots.
            int offloads = 0;
            for (int n = 1; n < roots.Count; n++)
            {
                int rootRow = roots[n];
                Card rootCard = fromPile[rootRow];
                int runLength = roots[n - 1] - roots[n];
                int suits = fromPile.CountSuits(rootRow, rootRow + runLength);
                int maxExtraSuits = Game.ExtraSuits(emptyPilesLeft);
                bool suitsMatch = false;
                holdingStack.Clear();

                // Try to find the best matching target.
                int to = -1;
                for (int i = 0; i < NumberOfPiles; i++)
                {
                    if (map[i].Face - 1 == rootCard.Face)
                    {
                        if (!offload.IsEmpty && to == offload.Column)
                        {
                            to = -1;
                            suitsMatch = false;
                        }
                        if (!suitsMatch && map[i].Suit == rootCard.Suit)
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
                    if (!offload.IsEmpty && to == offload.Column)
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
                        suits -= game.FindHolding(map, holdingStack, false, from, rootRow, rootRow + runLength, to, maxExtraSuits);
                        if (suits - 1 > maxExtraSuits)
                        {
                            // Not enough empty piles.
                            return;
                        }
                    }

                    // Record the order improvement.
                    order += Game.GetOrder(true, suitsMatch);
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
                            AddMove(MoveFlags.Empty, from, order);
                        }
                        return;
                    }

                    // Check for partial offload.
                    if (offloads > 0)
                    {
                        AddMove(MoveFlags.Empty, from, order);
                    }

                    // Try to offload this run.
                    if (emptyPilesLeft == 0)
                    {
                        // Not enough empty piles.
                        return;
                    }
                    to = EmptyPiles[0];
                    if (suits > maxExtraSuits)
                    {
                        // Try using holding piles.
                        suits -= game.FindHolding(map, holdingStack, false, from, rootRow, rootRow + runLength, to, maxExtraSuits);
                        if (suits > maxExtraSuits)
                        {
                            // Still not enough empty piles.
                            return;
                        }
                    }
                    int emptyPilesUsed = Game.EmptyPilesUsed(emptyPilesLeft, suits);
                    emptyPilesLeft -= emptyPilesUsed;
                    offload = new OffloadInfo(n, to, suits, emptyPilesUsed);
                    type = offload.SinglePile ? MoveType.Basic : MoveType.Unload;
                    isOffload = true;
                    offloads++;
                }

                // Extract the holding set.
                HoldingSet holdingSet = holdingStack.Set;
                bool undoHolding = !isOffload;
                int remainingLength = runLength - holdingSet.Length;

                if (undoHolding)
                {
                    // Add moves to the holding piles.
                    foreach (HoldingInfo holding in holdingSet.Forwards)
                    {
                        SupplementaryMoves.Add(new Move(MoveType.Basic, MoveFlags.Holding, from, -holding.Length, holding.To));
                    }

                    // Add the move.
                    SupplementaryMoves.Add(new Move(type, from, rootRow, to));

                    // Undo moves to the holding piles.
                    int toOffset = remainingLength;
                    foreach (HoldingInfo holding in holdingSet.Backwards)
                    {
                        SupplementaryMoves.Add(new Move(MoveType.Basic, MoveFlags.UndoHolding, holding.To, -holding.Length, to));
                        toOffset += holding.Length;
                    }
                    Debug.Assert(toOffset == runLength);

                    // Update the map.
                    map[to] = fromPile[rootRow + runLength - 1];
                }
                else
                {
                    // Add moves to the holding piles.
                    foreach (HoldingInfo holding in holdingSet.Forwards)
                    {
                        SupplementaryMoves.Add(new Move(MoveType.Basic, MoveFlags.Holding, from, -holding.Length, holding.To));

                        map[holding.To] = fromPile[holding.FromRow + holding.Length - 1];
                    }

                    // Add the move.
                    SupplementaryMoves.Add(new Move(type, from, rootRow, to));

                    // Update the map.
                    map[to] = fromPile[rootRow + remainingLength - 1];
                }

                if (rootRow == 0 && DownPiles[from].Count == 0)
                {
                    // Got to the bottom of the pile
                    // and created an empty pile.
                    emptyPilesLeft++;
                }

                CheckOffload(from, rootRow, to);
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
                    SupplementaryMoves.Add(new Move(MoveType.Reload, offload.Column, 0, from, 0));
                }
            }

            // Determine move type.
            int downCount = DownPiles[from].Count;
            MoveFlags flags = MoveFlags.Empty;
            if (downCount != 0)
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
            AddMove(flags, from, order);
        }

        private void CheckOffload(int from, int rootRow, int to)
        {
            if (offload.IsEmpty)
            {
                // No offload to check.
                return;
            }

            int offloadRootRow = roots[offload.Root];
            Card offloadRootCard = fromPile[offloadRootRow];
            int offloadSuits = offload.Suits;
            int offloadMaxExtraSuits = Game.ExtraSuits(emptyPilesLeft);
            bool matchesFrom = false;
            bool matchesTo = false;
            Card targetCard = Card.Empty;

            if (rootRow > 0 && offloadRootCard.Face + 1 == fromPile[rootRow - 1].Face)
            {
                // Offload matches from pile.
                matchesFrom = true;
                targetCard = fromPile[rootRow - 1];
            }

            if (offloadRootCard.Face + 1 == map[to].Face)
            {
                // Offoad matches to pile.
                matchesTo = true;
                targetCard = map[to];
            }

            if (!matchesFrom && !matchesTo)
            {
                return;
            }

            if (offload.SinglePile && offloadSuits - 1 > offloadMaxExtraSuits)
            {
#if false
                offloadSuits -= FindHolding(map, holdingStack, false, from, offloadRootRow, offloadRootRow + runLength, to, maxExtraSuits);
                if (offloadSuits - 1 > offloadMaxExtraSuits)
                {
                    // Can't move the offload due to additional suits.
                    continue;
                }
#endif
                return;
            }

            if (matchesFrom)
            {
                // Offload matches from pile.
                SupplementaryMoves.Add(new Move(offload.SinglePile ? MoveType.Basic : MoveType.Reload, offload.Column, 0, from));
                AddMove(MoveFlags.Empty, from, order + Game.GetOrder(targetCard, offloadRootCard));
                SupplementaryMoves.RemoveAt(SupplementaryMoves.Count - 1);
            }

            if (matchesTo)
            {
                // Record the order improvement.
                order += Game.GetOrder(targetCard, offloadRootCard);

                // Found a home for the offload.
                MoveType offloadType = offload.SinglePile ? MoveType.Basic : MoveType.Reload;
                SupplementaryMoves.Add(new Move(offloadType, offload.Column, 0, to));

                // Update the map.
                map[to] = map[offload.Column];
                map[offload.Column] = Card.Empty;

                // Update the state.
                emptyPilesLeft += offload.EmptyPilesUsed;
                offload = OffloadInfo.Empty;
            }
        }

        private void AddMove(MoveFlags flags, int from, int order)
        {
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
                        int currentOrder = order;
                        if (previousOrder >= currentOrder)
                        {
                            return;
                        }
                    }
                }

                // Clear out the previous best move.
                Candidates[best] = Move.Empty;
            }

            // Add the scoring move and the accumulated supplementary moves.
            best = Candidates.Count;
            Candidates.Add(new Move(MoveType.CompositeSinglePile, flags, from, 0, 0, order, -1, game.AddSupplementary()));
        }
    }
}
