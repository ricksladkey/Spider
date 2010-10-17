﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Spider.Collections;

namespace Spider.Engine
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