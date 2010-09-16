using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public struct PileInfo
    {
        public static PileInfo Empty = new PileInfo(Card.Empty, 0);

        public PileInfo(Card last, int count)
            : this()
        {
            Update(last, count);
        }

        public Card Last { get; set; }
        public int Count { get; set; }

        public void Update(Card last, int count)
        {
            Last = last;
            Count = count;
        }

        public void Update(Pile pile)
        {
            Update(pile, pile.Count);
        }

        public void Update(Pile pile, int count)
        {
            Count = count;
            if (Count == 0)
            {
                Last = Card.Empty;
            }
            else
            {
                Last = pile[Count - 1];
            }
        }

        public override string ToString()
        {
            return string.Format("Count: {0}, Last: {1}", Count, Last);
        }
    }
}
