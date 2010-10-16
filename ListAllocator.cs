using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public class ListAllocator<T>
    {
        public struct Allocation
        {
            public Allocation(T[] array, int offset)
            {
                Array = array;
                Offset = offset;
            }

            public T[] Array;
            public int Offset;
        }

        private List<T[]> arrays;
        private int current;
        private int offset;
        private int segmentSize;

        public ListAllocator()
        {
            segmentSize = 0x10000;
            arrays = new List<T[]>();
            arrays.Add(new T[segmentSize]);
            current = 0;
            offset = 0;
        }

        public void Clear()
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
            current = 0;
            offset = 0;
        }

        public Allocation Allocate(int count)
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
            return new Allocation(array, oldOffset);
        }
    }
}
