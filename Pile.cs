using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(CollectionDebugView))]
    public class Pile : SmallList<Card>
    {
        public Pile()
            : base(2 * 52)
        {
        }

        public Pile(IEnumerable<Card> other)
            : this()
        {
            foreach (Card card in other)
            {
                Add(card);
            }
        }

        public void Shuffle(int seed)
        {
            Random random = new Random(seed);
            // Knuth shuffle algorithm: for each card
            // except the last, swap it with one of the
            // later cards.
            for (int i = 0; i < Count - 1; i++)
            {
                int swap = random.Next(Count - i);
                Card tmp = this[i + swap];
                this[i + swap] = this[i];
                this[i] = tmp;
            }
        }
    }
}
