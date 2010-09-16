using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public struct PileInfo
    {
        public static PileInfo Empty = new PileInfo(0, Card.Empty, Card.Empty);

        public int Count { get; set; }
        public Card First { get; set; }
        public Card Last { get; set; }

        public PileInfo(int count, Card first, Card last)
            : this()
        {
            Count = count;
            First = first;
            Last = last;
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
                First = Card.Empty;
                Last = Card.Empty;
            }
            else
            {
                First = pile[0];
                Last = pile[Count - 1];
            }
        }

        public override string ToString()
        {
            return string.Format("Count: {0}, First: {1}, Last: {2}", Count, First, Last);
        }
    }
}
