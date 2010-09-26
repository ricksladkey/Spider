using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(EnumerableDebugView))]
    public class PileList : FastList<int>
    {
        public PileList()
            : base(Game.NumberOfPiles)
        {
        }

        public PileList(PileList other)
            : this()
        {
            AddRange(other);
        }
    }
}
