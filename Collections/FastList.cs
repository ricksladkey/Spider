using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider.Collections
{
    [DebuggerDisplay("Count = {count}")]
    [DebuggerTypeProxy(typeof(EnumerableDebugView))]
    public class FastList<T> : IList<T>, IStack<T>, IReadOnlyList<T>
    {
        protected int capacity;
        protected int count;
        protected T[] array;
        protected IEqualityComparer<T> comparer;

        public FastList()
            : this(10, 0)
        {
        }

        public FastList(int capacity)
            : this(capacity, 0)
        {
        }

        public FastList(int capacity, int count)
        {
            this.capacity = capacity;
            this.count = count;
            array = new T[capacity];
            comparer = EqualityComparer<T>.Default;
        }

        public void Copy(FastList<T> other)
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

        public void AddRange(FastList<T> other)
        {
            AddRange(other, 0, other.Count);
        }

        public void AddRange(IList<T> other)
        {
            AddRange(other, 0, other.Count);
        }

        public void AddRange(FastList<T> other, int index, int count)
        {
            if (capacity < this.count + count)
            {
                IncreaseCapacity(count);
            }
            for (int i = 0; i < count; i++)
            {
                array[this.count + i] = other.array[index + i];
            }
            this.count += count;
        }

        public void AddRange(IList<T> other, int index, int count)
        {
            if (capacity < this.count + count)
            {
                IncreaseCapacity(count);
            }
            for (int i = 0; i < count; i++)
            {
                array[this.count + i] = other[index + i];
            }
            this.count += count;
        }

        public void RemoveRange(int index, int count)
        {
            Debug.Assert(index >= 0 && index + count <= this.count);
            for (int i = index + count; i < count; i++)
            {
                array[i - count] = array[i];
            }
            this.count -= count;
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
            if (capacity < count + 1)
            {
                IncreaseCapacity(1);
            }
            for (int i = index; i < count; i++)
            {
                array[i + 1] = array[i];
            }
            array[index] = item;
            count++;
        }

        public void RemoveAt(int index)
        {
            Debug.Assert(index >= 0 && index < count);
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

        #region IStack<T> Members

        public void Push(T item)
        {
            Add(item);
        }

        public T Peek()
        {
            Debug.Assert(count > 0);
            return array[count - 1];
        }

        public T Pop()
        {
            Debug.Assert(count > 0);
            return array[--count];
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            if (capacity < count + 1)
            {
                IncreaseCapacity(1);
            }
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
            //this.array.CopyTo(array, index);
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

        #region IReadOnlyList<T> Members

        bool IReadOnlyList<T>.Contains(T item)
        {
            return Contains(item);
        }

        void IReadOnlyList<T>.CopyTo(T[] array, int index)
        {
            CopyTo(array, index);
        }

        int IReadOnlyList<T>.IndexOf(T item)
        {
            return IndexOf(item);
        }

        int IReadOnlyList<T>.Count
        {
            get
            {
                return count;
            }
        }

        IList<T> IReadOnlyList<T>.Items
        {
            get
            {
                return this;
            }
        }

        T IReadOnlyList<T>.this[int index]
        {
            get
            {
                return array[index];
            }
        }

        #endregion

        private void IncreaseCapacity(int additionalCapacity)
        {
            int newCapacity = (count + additionalCapacity) * 2;
            T[] newArray = new T[newCapacity];
            array.CopyTo(newArray, 0);

            capacity = newCapacity;
            array = newArray;
        }
    }
}
