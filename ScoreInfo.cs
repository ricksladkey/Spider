using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public struct ScoreInfo
    {
        public const int CreatesFreeCellScore = 1000;
        public const int BaseScore = 0;
        public const int UsesFreeCellScore = -1000;

        public double[] Coefficients { get; set; }
        public int Coefficient0 { get; set; }

        public int FaceValue { get; set; }
        public int NetRunLength { get; set; }
        public bool TurnsOverCard { get; set; }
        public bool CreatesFreeCell { get; set; }
        public bool UsesFreeCell { get; set; }
        public int DownCount { get; set; }
        public bool IsCompositeSinglePile { get; set; }
        public bool NoFreeCells { get; set; }
        public int OneRunDelta { get; set; }
        public int Uses { get; set; }
        public bool IsKing { get; set; }

        public double Score
        {
            get
            {
                double score = BaseScore +
                    CreatesFreeCellScore * (CreatesFreeCell ? 1 : 0) +
                    UsesFreeCellScore * (UsesFreeCell ? 1 : 0) +
                    FaceValue +
                    Coefficients[Coefficient0 + 0] * NetRunLength +
                    Coefficients[Coefficient0 + 1] * (TurnsOverCard ? 1 : 0) +
                    Coefficients[Coefficient0 + 2] * (TurnsOverCard ? 1 : 0) * DownCount +
                    Coefficients[Coefficient0 + 3] * (IsCompositeSinglePile ? 1 : 0) +
                    Coefficients[Coefficient0 + 4] * (NoFreeCells ? 1 : 0) * DownCount +
                    Coefficients[Coefficient0 + 5] * OneRunDelta;

                return score;
            }
        }

        public double LastResortScore
        {
            get
            {
                double score = BaseScore +
                    UsesFreeCellScore * (UsesFreeCell ? 1 : 0) +
                    Uses +
                    Coefficients[Coefficient0 + 0] * (TurnsOverCard ? 1 : 0) +
                    Coefficients[Coefficient0 + 1] * DownCount +
                    Coefficients[Coefficient0 + 2] * (TurnsOverCard ? 1 : 0) * DownCount +
                    Coefficients[Coefficient0 + 3] * (IsKing ? 1 : 0) +
                    Coefficients[Coefficient0 + 4] * (IsCompositeSinglePile ? 1 : 0);

                return score;
            }
        }

        public ScoreInfo(double[] coefficients, int coefficient0)
            : this()
        {
            Coefficients = coefficients;
            Coefficient0 = coefficient0;
        }
    }
}
