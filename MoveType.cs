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
        Unload = 3,
        Reload = 4,
        CompositeSinglePile = 5,
    }
}
