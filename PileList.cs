using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    [DebuggerDisplay("Count = {occupied}")]
    [DebuggerTypeProxy(typeof(PileListDebugView))]
    public class PileList : IList<int>
    {
        internal class PileListDebugView
        {
            private PileList pileList;

            public PileListDebugView(PileList pileList)
            {
                this.pileList = pileList;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public int[] Items
            {
                get
                {
                    int[] array = new int[pileList.Count];
                    pileList.CopyTo(array, 0);
                    return array;
                }
            }
        }

        int occupied;
        int[] piles;

        public PileList()
        {
            occupied = 0;
            piles = new int[10];
        }

        #region IList<int> Members

        public int IndexOf(int item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, int item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            for (int i = index + 1; i < occupied; i++)
            {
                piles[i - 1] = piles[i];
            }
            occupied--;
        }

        public int this[int index]
        {
            get
            {
                Debug.Assert(index >= 0 && index < occupied);
                return piles[index];
            }
            set
            {
                Debug.Assert(index >= 0 && index < occupied);
                piles[index] = value;
            }
        }

        #endregion

        #region ICollection<int> Members

        public void Add(int item)
        {
            piles[occupied++] = item;
        }

        public void Clear()
        {
            occupied = 0;
        }

        public bool Contains(int item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(int[] array, int arrayIndex)
        {
            for (int i = 0; i < occupied; i++)
            {
                array[arrayIndex + i] = piles[i];
            }
        }

        public int Count
        {
            get
            {
                return occupied;
            }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(int item)
        {
            for (int i = 0; i < occupied; i++)
            {
                if (piles[i] == item)
                {
                    RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region IEnumerable<int> Members

        public IEnumerator<int> GetEnumerator()
        {
            for (int i = 0; i < occupied; i++)
            {
                yield return piles[i];
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
