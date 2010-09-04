using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
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
                    return -1;
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

        public HoldingInfo this[int index]
        {
            get
            {
                if (Stack.Count == 0)
                {
                    return new HoldingInfo(-1, -1, 0);
                }
                return Stack[index];
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
