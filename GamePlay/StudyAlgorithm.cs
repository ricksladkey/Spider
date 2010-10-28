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

        public static double[] GetCoefficients(Variation Variation)
        {
            int suits = Variation.NumberOfSuits;
            switch (Variation.NumberOfSuits)
            {
                case 1:
                    return OneSuitCoefficients;

                case 2:
                    return TwoSuitCoefficients;

                case 4:
                    return FourSuitCoefficients;

                default:
                    throw new Exception("invalid number of suits");
            }
        }

        public StudyAlgorithm(Game game)
            : base(game)
        {
            BasicMoveFinder = new BasicMoveFinder(game);
            SwapMoveFinder = new SwapMoveFinder(game);
            CompositeSinglePileMoveFinder = new CompositeSinglePileMoveFinder(game);
            ScoreCalculator = new ScoreCalculator(game);
        }

        private BasicMoveFinder BasicMoveFinder { get; set; }
        private SwapMoveFinder SwapMoveFinder { get; set; }
        private CompositeSinglePileMoveFinder CompositeSinglePileMoveFinder { get; set; }
        private ScoreCalculator ScoreCalculator { get; set; }

        #region IAlgorithm Members

        public void Initialize()
        {
        }

        public IList<double> GetCoefficients()
        {
            return GetCoefficients(Variation);
        }

        public void SetCoefficients()
        {
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
            double score = ScoreCalculator.Calculate(move);
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
