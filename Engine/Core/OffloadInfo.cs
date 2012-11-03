using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Spider.Engine.Collections;

namespace Spider.Engine.Core
{
    public struct OffloadInfo
    {
        public static OffloadInfo Empty = new OffloadInfo(-1, -1);

        public OffloadInfo(int to, int numberOfSpacesUsed)
            : this()
        {
            To = to;
            NumberOfSpacesUsed = numberOfSpacesUsed;
        }

        public int To { get; set; }
        public int NumberOfSpacesUsed { get; set; }

        public bool SinglePile
        {
            get
            {
                return NumberOfSpacesUsed == 1;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return To == -1;
            }
        }
    }
}
