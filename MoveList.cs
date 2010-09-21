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
            : base(Game.MaximumMoves * 2)
        {
        }
    }
}
