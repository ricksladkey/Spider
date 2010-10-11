using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
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
        bool ComplexMoves { get; set; }
    }
}
