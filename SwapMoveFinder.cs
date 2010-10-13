using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public class SwapMoveFinder : GameHelper
    {
        private CardMap CardMap { get; set; }

        public SwapMoveFinder(Game game)
            : base(game)
        {
            CardMap = new CardMap();
        }

        public void Check(int from, int fromRow, int extraSuits, int maxExtraSuits)
        {
#if false
            if (extraSuits + 1 > maxExtraSuits + HoldingStack.Suits)
            {
                // Need at least one space or a holding pile to swap.
                return;
            }
#endif
            if (fromRow == 0 && FindTableau.GetDownCount(from) != 0)
            {
                // Would turn over a card.
                return;
            }
            Pile fromPile = FindTableau[from];
            Card fromCard = fromPile[fromRow];
            Card fromCardParent = Card.Empty;
            bool inSequence = true;
            if (fromRow != 0)
            {
                fromCardParent = fromPile[fromRow - 1];
                inSequence = fromCardParent.IsTargetFor(fromCard);
            }
            for (int to = 0; to < NumberOfPiles; to++)
            {
                Pile toPile = FindTableau[to];
                if (to == from || toPile.Count == 0)
                {
                    continue;
                }
                int splitRow = toPile.Count - RunLengthsAnySuit[to];
                int toRow = -1;
                if (inSequence)
                {
                    // Try to find from counterpart in the first to run.
                    toRow = splitRow + (int)(toPile[splitRow].Face - fromCard.Face);
                    if (toRow < splitRow || toRow >= toPile.Count)
                    {
                        // Sequence doesn't contain our counterpart.
                        continue;
                    }
                }
                else
                {
                    // Try to swap with both runs out of sequence.
                    toRow = splitRow;
                    if (fromRow != 0 && !fromCardParent.IsTargetFor(toPile[toRow]))
                    {
                        // Cards don't match.
                        continue;
                    }
                }
                if (toRow == 0)
                {
                    if (fromRow == 0)
                    {
                        // No point in swap both entire piles.
                        continue;
                    }
                    if (FindTableau.GetDownCount(to) != 0)
                    {
                        // Would turn over a card.
                        continue;
                    }
                }
                else if (!toPile[toRow - 1].IsTargetFor(fromCard))
                {
                    // Cards don't match.
                    continue;
                }

#if false
                int toSuits = toPile.CountSuits(toRow);
                HoldingStack forwardHoldingStack = new HoldingStack();
                if (extraSuits + toSuits > maxExtraSuits)
                {
                    // Check whether forward holding piles will help.
                    int forwardHoldingSuits = FindHolding(FindTableau, forwardHoldingStack, true, from, fromRow, fromPile.Count, to, maxExtraSuits);
                    if (extraSuits + toSuits > maxExtraSuits + forwardHoldingSuits)
                    {
                        // Prepare an accurate map.
                        CardMap.Update(FindTableau);
                        foreach (HoldingInfo holding in forwardHoldingStack.Set)
                        {
                            CardMap[holding.To] = fromPile[holding.FromRow + holding.Length - 1];
                        }

                        // Check whether reverse holding piles will help.
                        HoldingStack reverseHoldingStack = new HoldingStack();
                        int reverseHoldingSuits = FindHolding(CardMap, reverseHoldingStack, true, to, toRow, toPile.Count, from, maxExtraSuits);
                        if (extraSuits + toSuits > maxExtraSuits + forwardHoldingSuits + reverseHoldingSuits)
                        {
                            continue;
                        }

                        ProcessCandidate(new Move(MoveType.Swap, from, fromRow, to, toRow, AddHolding(forwardHoldingStack.Set, reverseHoldingStack.Set)));
                        continue;
                    }
                }

                // We've found a legal swap.
                Debug.Assert(toRow == 0 || toPile[toRow - 1].IsTargetFor(fromCard));
                Debug.Assert(fromRow == 0 || fromCardParent.IsTargetFor(toPile[toRow]));
                ProcessCandidate(new Move(MoveType.Swap, from, fromRow, to, toRow, AddHolding(forwardHoldingStack.Set)));
#else
                int toSuits = toPile.CountSuits(toRow);
                bool foundSwap = false;
                foreach (HoldingSet holdingSet in HoldingStack.Sets)
                {
                    if (holdingSet.Contains(to))
                    {
                        // The pile is already in use.
                        continue;
                    }
                    if (extraSuits + toSuits > maxExtraSuits + holdingSet.Suits)
                    {
                        // Not enough spaces.
                        continue;
                    }

                    // We've found a legal swap.
                    Debug.Assert(toRow == 0 || toPile[toRow - 1].IsTargetFor(fromCard));
                    Debug.Assert(fromRow == 0 || fromCardParent.IsTargetFor(toPile[toRow]));
                    ProcessCandidate(new Move(MoveType.Swap, from, fromRow, to, toRow, AddHolding(holdingSet)));
                    foundSwap = true;
                    break;
                }

#if false
                if (UseSearch)
                {
                    return;
                }
#endif

                if (!foundSwap)
                {
                    // Check whether reverse holding piles will help.
                    HoldingSet holdingSet = HoldingStack.Set;
                    if (!holdingSet.Contains(to))
                    {
                        // Prepare an accurate map.
                        CardMap.Update(FindTableau);
                        foreach (HoldingInfo holding in holdingSet)
                        {
                            CardMap[holding.To] = fromPile[holding.FromRow + holding.Length - 1];
                        }

                        HoldingStack reverseHoldingStack = new HoldingStack();
                        int reverseHoldingSuits = FindHolding(CardMap, reverseHoldingStack, true, toPile, to, toRow, toPile.Count, from, maxExtraSuits);
                        if (extraSuits + toSuits <= maxExtraSuits + holdingSet.Suits + reverseHoldingSuits)
                        {
                            ProcessCandidate(new Move(MoveType.Swap, from, fromRow, to, toRow, AddHolding(holdingSet, reverseHoldingStack.Set)));
                        }
                    }
                }
#endif
            }
        }
    }
}
