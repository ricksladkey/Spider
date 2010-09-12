using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public struct ScoreInfo
    {
        public double[] Coefficients { get; private set; }
        public int Coefficient0 { get; private set; }

        public int FaceValue { get; set; }
        public int NetRunLength { get; set; }
        public bool TurnsOverCard { get; set; }
        public bool CreatesFreeCell { get; set; }
        public int DownCount { get; set; }
        public bool IsOffload { get; set; }
        public bool NoFreeCells { get; set; }
        public int OneRunDelta { get; set; }

        public double Score
        {
            get
            {
                double score = 100000 + FaceValue +
                    Coefficients[Coefficient0 + 0] * NetRunLength +
                    Coefficients[Coefficient0 + 1] * (TurnsOverCard ? 1 : 0) +
                    Coefficients[Coefficient0 + 2] * (CreatesFreeCell ? 1 : 0) +
                    Coefficients[Coefficient0 + 3] * (TurnsOverCard ? 1 : 0) * DownCount +
                    Coefficients[Coefficient0 + 4] * (IsOffload ? 1 : 0) +
                    Coefficients[Coefficient0 + 5] * (NoFreeCells ? 1 : 0) * DownCount +
                    Coefficients[Coefficient0 + 6] * OneRunDelta;

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
