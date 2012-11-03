using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider.Collections
{
    public class ListAllocator<T>
    {
        private bool clearEntries;
        private List<T[]> arrays;
        private int current;
        private int offset;
        private int segmentSize;

        public ListAllocator(bool clearEntries)
        {
            this.clearEntries = clearEntries;
            this.segmentSize = 0x10000;
            this.arrays = new List<T[]>();
            this.arrays.Add(new T[segmentSize]);
            this.current = 0;
            this.offset = 0;
        }

        public void Clear()
        {
            if (clearEntries)
            {
                for (int i = 0; i < current; i++)
                {
                    T[] array = arrays[i];
                    for (int j = 0; j < segmentSize; j++)
                    {
                        array[j] = default(T);
                    }
                }
                {
                    T[] array = arrays[current];
                    for (int j = 0; j < offset; j++)
                    {
                        array[j] = default(T);
                    }
                }
            }
            current = 0;
            offset = 0;
        }

        public ArraySegment<T> Allocate(int count)
        {
            T[] array = arrays[current];
            if (offset + count > segmentSize)
            {
                current++;
                if (current == arrays.Count)
                {
                    arrays.Add(new T[segmentSize]);
                }
                array = arrays[current];
                offset = 0;
            }
            int oldOffset = offset;
            offset += count;
            return new ArraySegment<T>(array, oldOffset, count);
        }
    }
}
