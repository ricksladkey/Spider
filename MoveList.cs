using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    [DebuggerDisplay("Count = {occupied}")]
    [DebuggerTypeProxy(typeof(MoveListDebugView))]
    public class MoveList : IList<Move>
    {
        internal class MoveListDebugView
        {
            private MoveList moveList;

            public MoveListDebugView(MoveList moveList)
            {
                this.moveList = moveList;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Move[] Items
            {
                get
                {
                    Move[] array = new Move[moveList.Count];
                    moveList.CopyTo(array, 0);
                    return array;
                }
            }
        }

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
                Debug.Assert(index >= 0 && index < occupied);
                return moves[index];
            }
            set
            {
                Debug.Assert(index >= 0 && index < occupied);
                moves[index] = value;
            }
        }

        #endregion

        #region ICollection<Move> Members

        public void Add(Move item)
        {
            if (occupied == moves.Length)
            {
                Move[] newMoves = new Move[moves.Length * 2];
                moves.CopyTo(newMoves, 0);
                moves = newMoves;
            }
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
            for (int i = 0; i < occupied; i++)
            {
                array[arrayIndex + i] = moves[i];
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
