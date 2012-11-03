using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Spider.Engine.Collections;

namespace Spider.Engine.Core
{
    [DebuggerDisplay("Count = {Count}, From = {From}, FromRow = {FromRow}, To = {To}, Suits = {Suits}, Length = {Length}")]
    [DebuggerTypeProxy(typeof(EnumerableDebugView))]
    public struct HoldingSet : IEnumerable<HoldingInfo>
    {
        public HoldingSet(HoldingStack stack, int count)
            : this()
        {
            Stack = stack;
            Count = count;
        }

        public HoldingStack Stack { get; set; }
        public int Count { get; set; }

        public bool Contains(int column)
        {
            for (int i = 0; i < Count; i++)
            {
                if (Stack[i].To == column)
                {
                    return true;
                }
            }
            return false;
        }

        public int From
        {
            get
            {
                if (Count == 0)
                {
                    return -1;
                }
                return Stack[Count - 1].From;
            }
        }

        public int FromRow
        {
            get
            {
                if (Count == 0)
                {
                    return Stack.StartingRow;
                }
                return Stack[Count - 1].FromRow;
            }
        }

        public int To
        {
            get
            {
                if (Count == 0)
                {
                    return -1;
                }
                return Stack[Count - 1].To;
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
