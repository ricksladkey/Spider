using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public class BaseGame
    {
        public static int ExtraSuits(int emptyPiles)
        {
#if true
            // The formula for how many intermediate runs can
            // be moved is m: = sum(1 + 2 + ... + n).
            return emptyPiles * (emptyPiles + 1) / 2;
#else
            // The formula for how many intermediate runs can
            // be moved is m: = sum(1 + 2 + ... + 2^(n - 1)).
            if (emptyPiles < 0)
            {
                return 0;
            }
            int power = 1;
            for (int i = 0; i < emptyPiles; i++)
            {
                power *= 2;
            }
            return power - 1;
#endif
        }

        public static int EmptyPilesUsed(int emptyPiles, int suits)
        {
            int used = 0;
            for (int n = emptyPiles; n > 0 && suits > 0; n--)
            {
                used++;
                suits -= n;
            }
            return used;
        }

        private static int RoundUpExtraSuits(int suits)
        {
            int emptyPiles = 0;
            while (true)
            {
                int extraSuits = ExtraSuits(emptyPiles);
                if (extraSuits >= suits)
                {
                    return extraSuits;
                }
                emptyPiles++;
            }
        }

        public static int GetOrder(Card parent, Card child)
        {
            if (parent.Face - 1 != child.Face)
            {
                return 0;
            }
            if (parent.Suit != child.Suit)
            {
                return 1;
            }
            return 2;
        }

        public static int GetOrder(bool facesMatch, bool suitsMatch)
        {
            if (!facesMatch)
            {
                return 0;
            }
            if (!suitsMatch)
            {
                return 1;
            }
            return 2;
        }
    }
}
