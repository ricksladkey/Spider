using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Spider.Engine.Collections;
using Spider.Engine.Core;

namespace Spider.Engine.GamePlay
{
    public class ScoreCalculator : GameAdapter
    {
        public const int Group0 = 0;
        public const int Group1 = 9;

        public ScoreCalculator(Game game)
            : base(game)
        {
        }

        public double Calculate(Move move)
        {
            if (move.IsEmpty)
            {
                return Move.RejectScore;
            }

            ScoreInfo score = new ScoreInfo(Coefficients, Group0);

            int from = move.From;
            int fromRow = move.FromRow;
            int to = move.To;
            int toRow = move.ToRow;

            if (move.Type == MoveType.CompositeSinglePile)
            {
                return CalculateCompositeSinglePileScore(move);
            }
            Pile fromPile = FindTableau[from];
            Pile toPile = FindTableau[to];
            if (toPile.Count == 0)
            {
                return CalculateLastResortScore(move);
            }
            bool isSwap = move.Type == MoveType.Swap;
            Card fromParent = fromRow != 0 ? fromPile[fromRow - 1] : Card.Empty;
            Card fromChild = fromPile[fromRow];
            Card toParent = toRow != 0 ? toPile[toRow - 1] : Card.Empty;
            Card toChild = toRow != toPile.Count ? toPile[toRow] : Card.Empty;
            int oldOrderFrom = GetOrder(fromParent, fromChild);
            int newOrderFrom = GetOrder(toParent, fromChild);
            int oldOrderTo = isSwap ? GetOrder(toParent, toChild) : 0;
            int newOrderTo = isSwap ? GetOrder(fromParent, toChild) : 0;
            score.Order = newOrderFrom - oldOrderFrom + newOrderTo - oldOrderTo;
            if (score.Order < 0)
            {
                return Move.RejectScore;
            }
            score.Reversible = oldOrderFrom != 0 && (!isSwap || oldOrderTo != 0);
            score.Uses = CountUses(move);
            score.OneRunDelta = !isSwap ? RunFinder.GetOneRunDelta(oldOrderFrom, newOrderFrom, move) : 0;
            int faceFrom = (int)fromChild.Face;
            int faceTo = isSwap ? (int)toChild.Face : 0;
            score.FaceValue = Math.Max(faceFrom, faceTo);
            bool wholePile = fromRow == 0 && toRow == toPile.Count;
            int netRunLengthFrom = RunFinder.GetNetRunLength(newOrderFrom, from, fromRow, to, toRow);
            int netRunLengthTo = isSwap ? RunFinder.GetNetRunLength(newOrderTo, to, toRow, from, fromRow) : 0;
            score.NetRunLength = netRunLengthFrom + netRunLengthTo;
#if true
            int newRunLengthFrom = RunFinder.GetNewRunLength(newOrderFrom, from, fromRow, to, toRow);
            int newRunLengthTo = isSwap ? RunFinder.GetNewRunLength(newOrderTo, to, toRow, from, fromRow) : 0;
            score.Discards = newRunLengthFrom == 13 || newRunLengthTo == 13;
#endif
            score.DownCount = FindTableau.GetDownCount(from);
            score.TurnsOverCard = wholePile && score.DownCount != 0;
            score.CreatesSpace = wholePile && score.DownCount == 0;
            score.NoSpaces = FindTableau.NumberOfSpaces == 0;
            if (score.Order == 0 && score.NetRunLength < 0)
            {
                return Move.RejectScore;
            }
            int delta = 0;
            if (score.Order == 0 && score.NetRunLength == 0)
            {
                if (!isSwap && oldOrderFrom == 1 && newOrderFrom == 1)
                {
                    delta = RunFinder.GetRunDelta(from, fromRow, to, toRow);
                }
                if (delta <= 0)
                {
                    return Move.RejectScore;
                }
            }
            score.IsCompositeSinglePile = false;

            return score.Score;
        }

        private double CalculateCompositeSinglePileScore(Move move)
        {
            ScoreInfo score = new ScoreInfo(Coefficients, Group0);

            score.Order = move.ToRow;
            score.FaceValue = 0;
            score.NetRunLength = 0;
            score.DownCount = FindTableau.GetDownCount(move.From);
            score.TurnsOverCard = move.Flags.TurnsOverCard();
            score.CreatesSpace = move.Flags.CreatesSpace();
            score.UsesSpace = move.Flags.UsesSpace();
            score.Discards = move.Flags.Discards();
            score.IsCompositeSinglePile = true;
            score.NoSpaces = FindTableau.NumberOfSpaces == 0;
            score.OneRunDelta = 0;

            if (score.UsesSpace)
            {
                // XXX: should calculate uses, is king, etc.
                score.Coefficient0 = Group1;
                return score.LastResortScore;
            }
            return score.Score;
        }

        private double CalculateLastResortScore(Move move)
        {
            ScoreInfo score = new ScoreInfo(Coefficients, Group1);

            Pile fromPile = FindTableau[move.From];
            Pile toPile = FindTableau[move.To];
            Card fromCard = fromPile[move.FromRow];
            bool wholePile = move.FromRow == 0;
            score.UsesSpace = true;
            score.DownCount = FindTableau.GetDownCount(move.From);
            score.TurnsOverCard = wholePile && score.DownCount != 0;
            score.FaceValue = (int)fromCard.Face;
            score.IsKing = fromCard.Face == Face.King;
            score.Uses = CountUses(move);

            if (wholePile)
            {
                // Only move an entire pile if there
                // are more cards to be turned over.
                if (!score.TurnsOverCard)
                {
                    return Move.RejectScore;
                }
            }
            else if (fromPile[move.FromRow - 1].IsTargetFor(fromCard))
            {
                // No point in splitting consecutive cards
                // unless they are part of a multi-move
                // sequence.
                return Move.RejectScore;
            }

            return score.LastResortScore;
        }

        private int CountUses(Move move)
        {
            if (move.FromRow == 0 || move.ToRow != FindTableau[move.To].Count)
            {
                // No exposed card, no uses.
                return 0;
            }

            int uses = 0;

            Pile fromPile = FindTableau[move.From];
            Card fromCard = fromPile[move.FromRow];
            Card exposedCard = fromPile[move.FromRow - 1];
            if (!exposedCard.IsTargetFor(fromCard))
            {
                // Check whether the exposed card will be useful.
                int numberOfSpaces = FindTableau.NumberOfSpaces - 1;
                int maxExtraSuits = ExtraSuits(numberOfSpaces);
                int fromSuits = RunFinder.CountSuits(move.From, move.FromRow);
                for (int nextFrom = 0; nextFrom < NumberOfPiles; nextFrom++)
                {
                    if (nextFrom == move.From || nextFrom == move.To)
                    {
                        // Inappropriate column.
                        continue;
                    }
                    Pile nextFromPile = FindTableau[nextFrom];
                    if (nextFromPile.Count == 0)
                    {
                        // Column is empty.
                        continue;
                    }
                    int nextFromRow = nextFromPile.Count - RunFinder.GetRunUpAnySuit(nextFrom);
                    if (!nextFromPile[nextFromRow].IsSourceFor(exposedCard))
                    {
                        // Not the card we need.
                        continue;
                    }
                    int extraSuits = RunFinder.CountSuits(nextFrom, nextFromRow) - 1;
                    if (extraSuits <= maxExtraSuits)
                    {
                        // Card leads to a useful move.
                        uses++;
                    }

                    // Check whether the exposed run will be useful.
                    int upperFromRow = move.FromRow - RunFinder.GetRunUp(move.From, move.FromRow);
                    if (upperFromRow != move.FromRow)
                    {
                        Card upperFromCard = fromPile[upperFromRow];
                        uses += FaceLists[(int)upperFromCard.Face + 1].Count;
                    }
                }
            }
            return uses;
        }
    }
}
