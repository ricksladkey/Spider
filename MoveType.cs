using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public enum MoveType
    {
        Unknown = 0,
        Basic = 1,
        Swap = 2,
        Holding = 3,
        Unload = 4,
        Reload = 5,
        CompositeSinglePile = 6,
    }
}
