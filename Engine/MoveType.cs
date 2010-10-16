using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Spider.Collections;

namespace Spider.Engine
{
    public enum MoveType
    {
        Unknown = 0,
        Basic = 1,
        Swap = 2,
        Deal = 3,
        Discard = 4,
        TurnOverCard = 5,
        Unload = 6,
        Reload = 7,
        CompositeSinglePile = 8,
    }
}
