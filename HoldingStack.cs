using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    [DebuggerDisplay("Count = {Count}, FromRow = {FromRow}, Suits = {Suits}")]
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

        public int StartingRow { get; set; }

        public int FromRow
        {
            get
            {
                if (Count == 0)
                {
                    return StartingRow;
                }
                return array[Count - 1].FromRow;
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
