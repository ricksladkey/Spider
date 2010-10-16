using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider.Collections
{
    public interface IStack<T> : ICollection<T>, IEnumerable<T>, System.Collections.IEnumerable
    {
        void Push(T item);
        T Peek();
        T Pop();
    }
}
