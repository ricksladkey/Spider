using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public struct LayoutPart
    {
        public LayoutPart(int column, int count)
            : this()
        {
            Column = column;
            Count = count;
        }

        public int Column { get; set; }
        public int Count { get; set; }
    }
}
