using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public struct PileInfo
    {
        public int Count { get; set; }
        public Card Card { get; set; }

        public PileInfo(int count, Card card)
            : this()
        {
            Count = count;
            Card = card;
        }

        public void Update(Pile pile)
        {
            Count = pile.Count;
            Card = Count != 0 ? pile[Count - 1] : Card.Empty;
        }

        public void Update(Pile pile, int count)
        {
            Count = count;
            Card = Count != 0 ? pile[Count - 1] : Card.Empty;
        }

        public override string ToString()
        {
            return string.Format("Count: {0}, Card: {1}", Count, Card);
        }
    }
}
