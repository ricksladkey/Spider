using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    [DebuggerDisplay("Count = {occupied}")]
    [DebuggerTypeProxy(typeof(PileDebugView))]
    public class Pile : IList<Card>
    {
        internal class PileDebugView
        {
            private Pile pile;

            public PileDebugView(Pile pile)
            {
                this.pile = pile;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Card[] Items
            {
                get
                {
                    Card[] array = new Card[pile.Count];
                    pile.CopyTo(array, 0);
                    return array;
                }
            }
        }

        private int occupied;
        private Card[] pile;

        public Pile()
        {
            occupied = 0;
            pile = new Card[52 * 2];
        }

        public Pile(IEnumerable<Card> other)
            : this()
        {
            foreach (Card card in other)
            {
                Add(card);
            }
        }

        public void Shuffle(int seed)
        {
            Random random = new Random(seed);
            // Knuth shuffle algorithm: for each card
            // except the last, swap it with one of the
            // later cards.
            for (int i = 0; i < Count - 1; i++)
            {
                int swap = random.Next(Count - i);
                Card tmp = this[i + swap];
                this[i + swap] = this[i];
                this[i] = tmp;
            }
        }

        public void Copy(Pile other)
        {
            Clear();
            AddRange(other, 0, other.Count);
        }

        public void AddRange(IEnumerable<Card> cards)
        {
            foreach (Card card in cards)
            {
                Add(card);
            }
        }

        public void AddRange(Pile other)
        {
            AddRange(other, 0, other.Count);
        }

        public void AddRange(Pile other, int index, int count)
        {
            for (int i = 0; i < count; i++)
            {
                pile[occupied + i] = other[index + i];
            }
            occupied += count;
        }

        public void RemoveRange(int index, int count)
        {
            for (int i = index + count; i < occupied; i++)
            {
                pile[i - count] = pile[i];
            }
            occupied -= count;
        }

        public Card Next()
        {
            occupied--;
            return pile[occupied];
        }

        #region IList<Card> Members

        public int IndexOf(Card item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, Card item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            for (int i = index + 1; i < occupied; i++)
            {
                pile[i - 1] = pile[i];
            }
            occupied--;
        }

        public Card this[int index]
        {
            get
            {
                Debug.Assert(index >= 0 && index < occupied);
                return pile[index];
            }
            set
            {
                Debug.Assert(index >= 0 && index < occupied);
                pile[index] = value;
            }
        }

        #endregion

        #region ICollection<Card> Members

        public void Add(Card item)
        {
            pile[occupied++] = item;
        }

        public void Clear()
        {
            occupied = 0;
        }

        public bool Contains(Card item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Card[] array, int arrayIndex)
        {
            for (int i = 0; i < occupied; i++)
            {
                array[arrayIndex + i] = pile[i];
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

        public bool Remove(Card item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<Card> Members

        public IEnumerator<Card> GetEnumerator()
        {
            for (int i = 0; i < occupied; i++)
            {
                yield return pile[i];
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
