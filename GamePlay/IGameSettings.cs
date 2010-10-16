using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Spider.Collections;
using Spider.Engine;

namespace Spider.GamePlay
{
    public interface IGameSettings
    {
        Variation Variation { get; set; }
        int Seed { get; set; }
        double[] Coefficients { get; set; }
        bool Diagnostics { get; set; }
        bool Interactive { get; set; }
        int Instance { get; set; }
        bool UseSearch { get; set; }

        bool TraceMoves { get; set; }
        bool TraceStartFinish { get; set; }
        bool TraceDeals { get; set; }
        bool TraceSearch { get; set; }
        bool ComplexMoves { get; set; }
    }
}
