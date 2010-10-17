using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Spider.Collections;
using Spider.Engine;

namespace Spider.GamePlay
{
    public class BasicMoveFinder : GameAdapter
    {
        public BasicMoveFinder(Game game)
            : base(game)
        {
        }

        public void Find()
        {
            Candidates.Clear();
            SupplementaryList.Clear();

            int numberOfSpaces = FindTableau.NumberOfSpaces;
            int maxExtraSuits = ExtraSuits(numberOfSpaces);
            int maxExtraSuitsToSpace = ExtraSuits(numberOfSpaces - 1);

            for (int from = 0; from < NumberOfPiles; from++)
            {
                HoldingStack holdingStack = HoldingStacks[from];
                Pile fromPile = FindTableau[from];
                holdingStack.Clear();
                holdingStack.StartingRow = fromPile.Count;
                int extraSuits = 0;
                for (int fromRow = fromPile.Count - 1; fromRow >= 0; fromRow--)
                {
                    Card fromCard = fromPile[fromRow];
                    if (fromCard.IsEmpty)
                    {
                        break;
                    }
                    if (fromRow < fromPile.Count - 1)
                    {
                        Card previousCard = fromPile[fromRow + 1];
                        if (!previousCard.IsSourceFor(fromCard))
                        {
                            break;
                        }
                        if (fromCard.Suit != previousCard.Suit)
                        {
                            // This is a cross-suit run.
                            extraSuits++;
                            if (extraSuits > maxExtraSuits + holdingStack.Suits)
                            {
                                break;
                            }
                        }
                    }

                    // Add moves to other piles.
                    if (fromCard.Face < Face.King)
                    {
                        PileList piles = FaceLists[(int)fromCard.Face + 1];
                        for (int i = 0; i < piles.Count; i++)
                        {
                            for (int count = 0; count <= holdingStack.Count; count++)
                            {
                                HoldingSet holdingSet = new HoldingSet(holdingStack, count);
                                if (extraSuits > maxExtraSuits + holdingSet.Suits)
                                {
                                    continue;
                                }
                                int to = piles[i];
                                if (from == to || holdingSet.Contains(from))
                                {
                                    continue;
                                }

                                // We've found a legal move.
                                Pile toPile = FindTableau[to];
                                Algorithm.ProcessCandidate(new Move(from, fromRow, to, toPile.Count, AddHolding(holdingSet)));

                                // Update the holding pile move.
                                int holdingSuits = extraSuits;
                                if (fromRow > 0 && (!fromPile[fromRow - 1].IsTargetFor(fromCard) || fromCard.Suit != fromPile[fromRow - 1].Suit))
                                {
                                    holdingSuits++;
                                }
                                if (holdingSuits > holdingStack.Suits)
                                {
                                    int length = holdingStack.FromRow - fromRow;
                                    holdingStack.Push(new HoldingInfo(from, fromRow, to, holdingSuits, length));
                                }

                                break;
                            }
                        }
                    }

                    // Add moves to an space.
                    for (int i = 0; i < FindTableau.NumberOfSpaces; i++)
                    {
                        int to = FindTableau.Spaces[i];

                        if (fromRow == 0)
                        {
                            // No point in moving from a full pile
                            // from one open position to another unless
                            // there are more cards to turn over.
                            if (FindTableau.GetDownCount(from) == 0)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            // No point in moving anything less than
                            // as much as possible to an space.
                            Card nextCard = fromPile[fromRow - 1];
                            if (fromCard.Suit == nextCard.Suit)
                            {
                                if (nextCard.IsTargetFor(fromCard))
                                {
                                    continue;
                                }
                            }
                        }

                        for (int count = 0; count <= holdingStack.Count; count++)
                        {
                            HoldingSet holdingSet = new HoldingSet(holdingStack, count);
                            if (holdingSet.FromRow == fromRow)
                            {
                                // No cards left to move.
                                continue;
                            }
                            if (extraSuits > maxExtraSuitsToSpace + holdingSet.Suits)
                            {
                                // Not enough spaces.
                                continue;
                            }

                            // We've found a legal move.
                            Pile toPile = FindTableau[to];
                            Algorithm.ProcessCandidate(new Move(from, fromRow, to, toPile.Count, AddHolding(holdingSet)));
                            break;
                        }

                        // Only need to check the first space
                        // since all spaces are the same
                        // except for undealt cards.
                        break;
                    }
                }
            }
        }
    }
}
