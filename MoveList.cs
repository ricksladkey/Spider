using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public class MoveList : IList<Move>
    {
        int occupied;
        private Move[] moves;

        public MoveList()
        {
            moves = new Move[1000];
        }

        #region IList<Move> Members

        public int IndexOf(Move item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, Move item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public Move this[int index]
        {
            get
            {
                return moves[index];
            }
            set
            {
                moves[index] = value;
            }
        }

        #endregion

        #region ICollection<Move> Members

        public void Add(Move item)
        {
            moves[occupied++] = item;
        }

        public void Clear()
        {
            occupied = 0;
        }

        public bool Contains(Move item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Move[] array, int arrayIndex)
        {
            throw new NotImplementedException();
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

        public bool Remove(Move item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<Move> Members

        public IEnumerator<Move> GetEnumerator()
        {
            for (int i = 0; i < occupied; i++)
            {
                yield return moves[i];
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
