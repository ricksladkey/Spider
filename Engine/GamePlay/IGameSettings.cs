using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Spider.Engine.Collections;
using Spider.Engine.Core;

namespace Spider.Engine.GamePlay
{
    public interface IGameSettings
    {
        Variation Variation { get; set; }
        AlgorithmType AlgorithmType { get; set; }
        int Seed { get; set; }
        double[] Coefficients { get; set; }
        bool Diagnostics { get; set; }
        bool Interactive { get; set; }
        int Instance { get; set; }

        bool TraceMoves { get; set; }
        bool TraceStartFinish { get; set; }
        bool TraceDeals { get; set; }
        bool TraceSearch { get; set; }
        bool ComplexMoves { get; set; }
    }
}
