using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public struct OffloadInfo
    {
        public static OffloadInfo Empty = new OffloadInfo(-1, -1, -1, -1);

        public OffloadInfo(int root, int pile, int suits, int freeCells)
            : this()
        {
            Root = root;
            Pile = pile;
            Suits = suits;
            FreeCells = freeCells;
            CanInvert = freeCells == 1;
        }

        public int Root { get; set; }
        public int Pile { get; set; }
        public int Suits { get; set; }
        public int FreeCells { get; set; }
        public bool CanInvert { get; set; }

        public bool IsEmpty
        {
            get
            {
                return Root == -1;
            }
        }
    }
}
