using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Spider.Engine.Collections;

namespace Spider.Engine.Core
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
