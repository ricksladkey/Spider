using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public struct OffloadInfo
    {
        public static OffloadInfo Empty = new OffloadInfo(-1, -1, -1, -1);

        public OffloadInfo(int root, int pile, int suits, int emptyPilesUsed)
            : this()
        {
            Root = root;
            Pile = pile;
            Suits = suits;
            EmptyPilesUsed = emptyPilesUsed;
        }

        public int Root { get; set; }
        public int Pile { get; set; }
        public int Suits { get; set; }
        public int EmptyPilesUsed { get; set; }

        public bool SinglePile
        {
            get
            {
                return EmptyPilesUsed == 1;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return Root == -1;
            }
        }
    }
}
