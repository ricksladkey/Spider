using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(CollectionDebugView))]
    public class PileList : SmallList<int>
    {
        public PileList()
            : base(Game.NumberOfPiles)
        {
        }
    }
}
