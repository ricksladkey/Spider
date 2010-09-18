using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public struct OffloadInfo
    {
        public static OffloadInfo Empty = new OffloadInfo(-1, -1, -1, -1, -1);

        public OffloadInfo(int root, int pile, int suits, int freeCells, int move)
            : this()
        {
            Root = root;
            Pile = pile;
            Suits = suits;
            FreeCells = freeCells;
            Move = move;
        }

        public int Root { get; set; }
        public int Pile { get; set; }
        public int Suits { get; set; }
        public int FreeCells { get; set; }
        public int Move { get; set; }

        public bool IsEmpty
        {
            get
            {
                return Root == -1;
            }
        }
    }
}
