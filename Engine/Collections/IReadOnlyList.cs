using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider.Collections
{
    public interface IReadOnlyList<T>
    {
        int Count { get; }
        IList<T> Items { get; }
        T this[int index] { get; }
        bool Contains(T item);
        void CopyTo(T[] array, int index);
        int IndexOf(T item);
    }
}
