using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    [DebuggerDisplay("Count = {count}")]
    [DebuggerTypeProxy(typeof(CollectionDebugView))]
    public class SmallList<T> : IList<T>
    {
        private int count;
        private T[] array;
        private IEqualityComparer<T> comparer;

        public SmallList(int capacity)
        {
            count = 0;
            array = new T[capacity];
            comparer = EqualityComparer<T>.Default;
        }

        public void Copy(SmallList<T> other)
        {
            Clear();
            AddRange(other, 0, other.Count);
        }

        public void Copy(IList<T> other)
        {
            Clear();
            AddRange(other, 0, other.Count);
        }

        public void AddRange(IEnumerable<T> Ts)
        {
            foreach (T item in Ts)
            {
                Add(item);
            }
        }

        public void AddRange(SmallList<T> other)
        {
            AddRange(other, 0, other.Count);
        }

        public void AddRange(IList<T> other)
        {
            AddRange(other, 0, other.Count);
        }

        public void AddRange(SmallList<T> other, int index, int count)
        {
            for (int i = 0; i < count; i++)
            {
                array[this.count + i] = other.array[index + i];
            }
            this.count += count;
        }

        public void AddRange(IList<T> other, int index, int count)
        {
            for (int i = 0; i < count; i++)
            {
                array[this.count + i] = other[index + i];
            }
            this.count += count;
        }

        public void RemoveRange(int index, int count)
        {
            for (int i = index + count; i < count; i++)
            {
                array[i - count] = array[i];
            }
            this.count -= count;
        }

        public T Next()
        {
            return array[--count];
        }

        #region IList<T> Members

        public int IndexOf(T item)
        {
            for (int i = 0; i < count; i++)
            {
                if (comparer.Equals(array[i], item))
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            for (int i = index; i < count; i++)
            {
                array[i + 1] = array[i];
            }
            array[index] = item;
            count++;
        }

        public void RemoveAt(int index)
        {
            for (int i = index + 1; i < count; i++)
            {
                array[i - 1] = array[i];
            }
            count--;
        }

        public T this[int index]
        {
            get
            {
                Debug.Assert(index >= 0 && index < count);
                return array[index];
            }
            set
            {
                Debug.Assert(index >= 0 && index < count);
                array[index] = value;
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            array[count++] = item;
        }

        public void Clear()
        {
            count = 0;
        }

        public bool Contains(T item)
        {
            return IndexOf(item) != -1;
        }

        public void CopyTo(T[] array, int index)
        {
            for (int i = 0; i < count; i++)
            {
                array[index + i] = this.array[i];
            }
        }

        public int Count
        {
            get
            {
                return count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(T item)
        {
            for (int i = 0; i < count; i++)
            {
                if (comparer.Equals(array[i], item))
                {
                    RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < count; i++)
            {
                yield return array[i];
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
