using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Spider.Collections;

namespace Spider.Engine
{
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(EnumerableDebugView))]
    public class PileList : FastList<int>
    {
        public PileList()
            : base(10)
        {
        }

        public PileList(IList<int> other)
            : this()
        {
            AddRange(other);
        }
    }
}
