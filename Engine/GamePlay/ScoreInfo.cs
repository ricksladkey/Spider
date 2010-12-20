using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Spider.Engine.Collections;
using Spider.Engine.Core;

namespace Spider.Engine.GamePlay
{
    public struct ScoreInfo
    {
        public ScoreInfo(double[] coefficients, int coefficient0)
            : this()
        {
            Coefficients = coefficients;
            Coefficient0 = coefficient0;
        }
        
        public const int CreatesSpaceScore = 1000;
        public const int BaseScore = 0;
        public const int UsesSpaceScore = -1000;
        public const int ReversibleScore = 500;

        public double[] Coefficients { get; set; }
        public int Coefficient0 { get; set; }

        public bool Reversible { get; set; }
        public int Order { get; set; }
        public int FaceValue { get; set; }
        public int NetRunLength { get; set; }
        public bool TurnsOverCard { get; set; }
        public bool CreatesSpace { get; set; }
        public bool UsesSpace { get; set; }
        public bool Discards { get; set; }
        public int DownCount { get; set; }
        public bool IsCompositeSinglePile { get; set; }
        public bool NoSpaces { get; set; }
        public int OneRunDelta { get; set; }
        public int Uses { get; set; }
        public bool IsKing { get; set; }

        public double Score
        {
            get
            {
                double score = BaseScore +
                    ReversibleScore * (Reversible ? 1 : 0) +
                    CreatesSpaceScore * (CreatesSpace ? 1 : 0) +
                    UsesSpaceScore * (UsesSpace ? 1 : 0) +
                    FaceValue +
                    Coefficients[Coefficient0 + 0] * NetRunLength +
                    Coefficients[Coefficient0 + 1] * (TurnsOverCard ? 1 : 0) +
                    Coefficients[Coefficient0 + 2] * (TurnsOverCard ? 1 : 0) * DownCount +
                    Coefficients[Coefficient0 + 3] * (IsCompositeSinglePile ? 1 : 0) +
                    Coefficients[Coefficient0 + 4] * (NoSpaces ? 1 : 0) * DownCount +
                    Coefficients[Coefficient0 + 5] * OneRunDelta +
                    Coefficients[Coefficient0 + 6] * Uses +
                    Coefficients[Coefficient0 + 7] * Order +
                    Coefficients[Coefficient0 + 8] * (Discards ? 1 : 0);

                return score;
            }
        }

        public double LastResortScore
        {
            get
            {
                double score = BaseScore +
                    UsesSpaceScore * (UsesSpace ? 1 : 0) +
                    Uses +
                    Coefficients[Coefficient0 + 0] * (TurnsOverCard ? 1 : 0) +
                    Coefficients[Coefficient0 + 1] * DownCount +
                    Coefficients[Coefficient0 + 2] * (TurnsOverCard ? 1 : 0) * DownCount +
                    Coefficients[Coefficient0 + 3] * (IsKing ? 1 : 0) +
                    Coefficients[Coefficient0 + 4] * (IsCompositeSinglePile ? 1 : 0) +
                    Coefficients[Coefficient0 + 5] * Order;

                return score;
            }
        }
    }
}
