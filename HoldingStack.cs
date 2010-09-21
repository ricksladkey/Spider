using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    [DebuggerDisplay("Count = {Count}, Index = {Index}, Suits = {Suits}")]
    [DebuggerTypeProxy(typeof(EnumerableDebugView))]
    public class HoldingStack : FastList<HoldingInfo>
    {
        public HoldingStack()
            : base(Game.NumberOfPiles)
        {
        }

        public void Push(HoldingInfo item)
        {
            Add(item);
        }

        public HoldingInfo Pop()
        {
            HoldingInfo item = array[Count - 1];
            RemoveAt(Count - 1);
            return item;
        }

        public int StartingIndex { get; set; }

        public int Index
        {
            get
            {
                if (Count == 0)
                {
                    return StartingIndex;
                }
                return array[Count - 1].FromIndex;
            }
        }

        public int Suits
        {
            get
            {
                if (Count == 0)
                {
                    return 0;
                }
                return array[Count - 1].Suits;
            }
        }

        public HoldingSet Set
        {
            get
            {
                return new HoldingSet(this, Count);
            }
        }

        public IEnumerable<HoldingSet> Sets
        {
            get
            {
                for (int i = 0; i <= Count; i++)
                {
                    yield return new HoldingSet(this, i);
                }
            }
        }
    }
}
