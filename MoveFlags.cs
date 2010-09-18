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
        Complete = 0x0001,
    }
}
