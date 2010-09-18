using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    [Flags]
    public enum MoveFlags
    {
        Empty = 0x0000,
        CreatesFreeCell = 0x0001,
        TurnsOverCard = 0x0002,
        UsesFreeCell = 0x0004,
    }
}
