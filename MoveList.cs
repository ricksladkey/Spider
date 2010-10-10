using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(EnumerableDebugView))]
    public class MoveList : FastList<Move>
    {
        public MoveList()
        {
        }

        public MoveList(MoveList other)
        {
            Copy(other);
        }
    }
}
