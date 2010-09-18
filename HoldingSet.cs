using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    [DebuggerDisplay("Count = {Count}, Pile = {Pile}, Index = {Index}, Suits = {Suits}, Length = {Length}")]
    [DebuggerTypeProxy(typeof(EnumerableDebugView))]
    public struct HoldingSet : IEnumerable<HoldingInfo>
    {
        public HoldingStack Stack { get; set; }
        public int Count { get; set; }

        public HoldingSet(HoldingStack stack, int count)
            : this()
        {
            Stack = stack;
            Count = count;
        }

        public bool Contains(int pile)
        {
            for (int i = 0; i < Count; i++)
            {
                if (Stack[i].Pile == pile)
                {
                    return true;
                }
            }
            return false;
        }

        public int Pile
        {
            get
            {
                if (Count == 0)
                {
                    return -1;
                }
                return Stack[Count - 1].Pile;
            }
        }

        public int Index
        {
            get
            {
                if (Count == 0)
                {
                    return Stack.StartingIndex;
                }
                return Stack[Count - 1].Index;
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
                return Stack[Count - 1].Suits;
            }
        }

        public int Length
        {
            get
            {
                int length = 0;
                for (int i = 0; i < Count; i++)
                {
                    length += Stack[i].Length;
                }
                return length;
            }
        }

        public HoldingInfo this[int index]
        {
            get
            {
                if (Stack.Count == 0)
                {
                    return HoldingInfo.Empty;
                }
                return Stack[index];
            }
        }

        public IEnumerable<HoldingInfo> Forwards
        {
            get
            {
                for (int i = 0; i < Count; i++)
                {
                    yield return Stack[i];
                }
            }
        }

        public IEnumerable<HoldingInfo> Backwards
        {
            get
            {
                for (int i = Count - 1; i >= 0; i--)
                {
                    yield return Stack[i];
                }
            }
        }

        #region IEnumerable<HoldingInfo> Members

        public IEnumerator<HoldingInfo> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return Stack[i];
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
