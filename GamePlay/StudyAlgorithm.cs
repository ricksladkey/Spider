using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Spider.Collections;
using Spider.Engine;

namespace Spider.GamePlay
{
    public class StudyAlgorithm : GameAdapter, IAlgorithm
    {
        public static double[] FourSuitCoefficients = new double[] {
            /* 0 */ 4.97385808, 63.53977337, -0.07690241043, -3.361553585, -0.2933748314, 1.781253839, 4.819874539, 0.4819874538, 86.27048442,
            /* 9 */ 4.465708423, 0.001610653073, -0.1302184743, -0.9577011316, 2.95155848, 0.7840526817,
        };

        public static double[] TwoSuitCoefficients = new double[] {
            /* 0 */ 5.633744758, 80.97892108, -0.05372285251, -3.999455611, -0.9077026719, 0.8480919033, 9.447113329, 1, 76.38970958,
            /* 9 */ 4.191362497, 4.048432827E-05, -0.03960051729, -0.1601725542, 0.7790220167, 0.4819874539,
        };

        public static double[] OneSuitCoefficients = new double[] {
            /* 0 */ 4.241634919, 93.31341988, -0.08091391227, -3.265541832, -0.5942021654, 2.565712243, 17.64117551, 1, 110.0314895,
            /* 9 */ 1.756489081, 0.0002561898898, -0.04347481483, -0.1737026135, 3.471266012, 1,
        };

        public const int Group0 = 0;
        public const int Group1 = 9;

        public StudyAlgorithm(Game game)
            : base(game)
        {
            BasicMoveFinder = new BasicMoveFinder(game);
            SwapMoveFinder = new SwapMoveFinder(game);
            CompositeSinglePileMoveFinder = new CompositeSinglePileMoveFinder(game);
        }

        private BasicMoveFinder BasicMoveFinder { get; set; }
        private SwapMoveFinder SwapMoveFinder { get; set; }
        private CompositeSinglePileMoveFinder CompositeSinglePileMoveFinder { get; set; }

        #region IAlgorithm Members

        public void SetCoefficients()
        {
            int suits = Variation.NumberOfSuits;
            if (suits == 1)
            {
                SetDefaultCoefficients(OneSuitCoefficients);
            }
            else if (suits == 2)
            {
                SetDefaultCoefficients(TwoSuitCoefficients);
            }
            else if (suits == 4)
            {
                SetDefaultCoefficients(FourSuitCoefficients);
            }
            else
            {
                throw new Exception("invalid number of suits");
            }
        }

        public void PrepareToPlay()
        {
        }

        public void FindMoves(Tableau tableau)
        {
            PrepareToFindMoves(tableau);
            BasicMoveFinder.Find();
            SwapMoveFinder.Find();
            CompositeSinglePileMoveFinder.Find();
        }

        public void MakeMove()
        {
            FindMoves(Tableau);
            ChooseMove();
        }

        public void ProcessCandidate(Move move)
        {
            double score = CalculateScore(move);
            if (score == Move.RejectScore)
            {
                return;
            }
            move.Score = score;
            Candidates.Add(move);
        }

        public void PrepareToDeal()
        {
        }

        public void RespondToDeal()
        {
        }

        #endregion

        private double CalculateScore(Move move)
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
                    int nextFromRow = nextFromPile.Count - RunFinder.GetRunLengthAnySuit(nextFrom);
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

        public void ChooseMove()
        {
            // We may be strictly out of moves.
            if (Candidates.Count == 0)
            {
                return;
            }

            if (Diagnostics)
            {
                PrintGame();
                PrintViableCandidates();
                Utils.WriteLine("Moves.Count = {0}", Tableau.Moves.Count);
            }

            Move move = Candidates[0];
            for (int i = 0; i < Candidates.Count; i++)
            {
                if (Candidates[i].Score > move.Score)
                {
                    move = Candidates[i];
                }
            }

            // The best move may not be worth making.
            if (move.Score == Move.RejectScore)
            {
                return;
            }

            ProcessMove(move);
        }
    }
}
