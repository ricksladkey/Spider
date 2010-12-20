using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Spider.Engine.Collections;

namespace Spider.Engine.Core
{
    public class CoreBase
    {
        public static int ExtraSuits(int numberOfSpaces)
        {
#if true
            // The formula for how many intermediate runs can
            // be moved is m: = sum(1 + 2 + ... + n).
            return numberOfSpaces * (numberOfSpaces + 1) / 2;
#else
            // The formula for how many intermediate runs can
            // be moved is m: = sum(1 + 2 + ... + 2^(n - 1)).
            if (numberOfSpaces < 0)
            {
                return 0;
            }
            int power = 1;
            for (int i = 0; i < numberOfSpaces; i++)
            {
                power *= 2;
            }
            return power - 1;
#endif
        }

        public static int SpacesUsed(int numberOfSpaces, int suits)
        {
#if false
            int used = 0;
            for (int n = numberOfSpaces; n > 0 && suits > 0; n--)
            {
                used++;
                suits -= n;
            }
            return used;
#else
            int used = 0;
            while (suits > 0)
            {
                suits -= ExtraSuits(numberOfSpaces - 1) + 1;
                used++;
                numberOfSpaces--;
            }
            return used;
#endif
        }

        private static int RoundUpExtraSuits(int suits)
        {
            int numberOfSpaces = 0;
            while (true)
            {
                int extraSuits = ExtraSuits(numberOfSpaces);
                if (extraSuits >= suits)
                {
                    return extraSuits;
                }
                numberOfSpaces++;
            }
        }

        public static int GetOrder(Card parent, Card child)
        {
            if (!parent.IsTargetFor(child))
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
