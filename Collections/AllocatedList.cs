using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider.Collections
{
    /// <summary>
    /// An AllocatedList is a list that uses a ListAllocator
    /// to allocate its storage.  This can be used to avoid
    /// memory allocation by combining multiple lists into
    /// a smaller number of pre-allocated underlying arrays.
    /// </summary>
    /// <typeparam name="T">The type of the list item.</typeparam>
    [DebuggerDisplay("Count = {count}")]
    [DebuggerTypeProxy(typeof(EnumerableDebugView))]
    public class AllocatedList<T> : IList<T>
    {
        private ListAllocator<T> allocator;
        private T[] array;
        private int offset;
        private int capacity;
        private int count;
        protected IEqualityComparer<T> comparer;

        public AllocatedList(ListAllocator<T> allocator, int capacity)
        {
            this.allocator = allocator;
            ListAllocator<T>.Allocation allocation = allocator.Allocate(capacity);
            this.array = allocation.Array;
            this.offset = allocation.Offset;
            this.capacity = capacity;
            this.count = 0;
            this.comparer = EqualityComparer<T>.Default;
        }

        public AllocatedList(ListAllocator<T> allocator, int capacity, int count)
            : this(allocator, capacity)
        {
            this.count = count;
        }

        public AllocatedList(ListAllocator<T> allocator)
            : this(allocator, 10)
        {
        }

        public AllocatedList(ListAllocator<T> allocator, IList<T> other)
            : this(allocator, other.Count)
        {
            AddRange(other, 0, other.Count);
        }

        public void Copy(IList<T> other)
        {
            Clear();
            AddRange(other, 0, other.Count);
        }

        public void AddRange(IList<T> other)
        {
            AddRange(other, 0, other.Count);
        }

        public void AddRange(IList<T> other, int index, int count)
        {
            if (capacity < this.count + count)
            {
                IncreaseCapacity(count);
            }
            for (int i = 0; i < count; i++)
            {
                array[this.count + i + offset] = other[index + i];
            }
            this.count += count;
        }

        #region IList<T> Members

        public int IndexOf(T item)
        {
            for (int i = 0; i < count; i++)
            {
                if (array[i + offset].Equals(item))
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
                array[i + 1 + offset] = array[i + offset];
            }
            array[index + offset] = item;
            count++;
        }

        public void RemoveAt(int index)
        {
            Debug.Assert(index >= 0 && index < count);
            for (int i = index + 1; i < count; i++)
            {
                array[i - 1 + offset] = array[i + offset];
            }
            count--;
        }

        public T this[int index]
        {
            get
            {
                Debug.Assert(index >= 0 && index < count);
                return array[index + offset];
            }
            set
            {
                Debug.Assert(index >= 0 && index < count);
                array[index + offset] = value;
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            if (capacity < count + 1)
            {
                IncreaseCapacity(1);
            }
            array[count + offset] = item;
            count++;
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
                array[index + i] = this.array[i + offset];
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
                if (comparer.Equals(array[i + offset], item))
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
                yield return array[i + offset];
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        private void IncreaseCapacity(int additionalCapacity)
        {
            int newCapacity = (count + additionalCapacity) * 2;
            ListAllocator<T>.Allocation allocation = allocator.Allocate(newCapacity);
            T[] newArray = allocation.Array;
            int newOffset = allocation.Offset;
            for (int i = 0; i < count; i++)
            {
                newArray[i + newOffset] = array[i + offset];
            }
            array = newArray;
            offset = newOffset;
            capacity = newCapacity;
        }
    }
}
