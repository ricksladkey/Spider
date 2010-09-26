using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(EnumerableDebugView))]
    public class Pile : FastList<Card>
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

        public Card LastCard
        {
            get
            {
                return count == 0 ? Card.Empty : array[count - 1];
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
                Card tmp = array[i + swap];
                array[i + swap] = array[i];
                array[i] = tmp;
            }
        }

        public int GetRunUp(int index)
        {
            if (index == 0)
            {
                return 0;
            }
            Debug.Assert(index >= 0 && index <= Count);
            int runLength = 1;
            for (int i = index - 2; i >= 0; i--)
            {
                Card card = array[i];
                Card nextCard = array[i + 1];
                if (nextCard.Suit != card.Suit)
                {
                    break;
                }
                if (nextCard.Face + 1 != card.Face)
                {
                    break;
                }
                runLength++;
            }
            return runLength;
        }

        public int GetRunUpAnySuit(int index)
        {
            if (index == 0)
            {
                return 0;
            }
            Debug.Assert(index >= 0 && index <= Count);
            int runLength = 1;
            for (int i = index - 2; i >= 0; i--)
            {
                Card card = array[i];
                Card nextCard = array[i + 1];
                if (nextCard.Face + 1 != card.Face)
                {
                    break;
                }
                runLength++;
            }
            return runLength;
        }

        public int GetRunDown(int index)
        {
            Debug.Assert(index >= 0 && index <= Count);
            if (index == Count)
            {
                return 0;
            }
            int runLength = 1;
            for (int i = index + 1; i < Count; i++)
            {
                Card previousCard = array[i - 1];
                Card card = array[i];
                if (previousCard.Suit != card.Suit)
                {
                    break;
                }
                if (previousCard.Face - 1 != card.Face)
                {
                    break;
                }
                runLength++;
            }
            return runLength;
        }

        public int GetRunDownAnySuit(int index)
        {
            if (index == Count)
            {
                return 0;
            }
            int runLength = 1;
            for (int i = index + 1; i < Count; i++)
            {
                Card previousCard = array[i - 1];
                Card card = array[i];
                if (previousCard.Face - 1 != card.Face)
                {
                    break;
                }
                runLength++;
            }
            return runLength;
        }

        public int CountSuits(int index)
        {
            return CountSuits(index, -1);
        }

        public int CountSuits(int startIndex, int endIndex)
        {
            if (endIndex == -1)
            {
                endIndex = Count;
            }
            Debug.Assert(startIndex >= 0 && startIndex <= Count);
            Debug.Assert(endIndex >= 0 && endIndex <= Count);
            int suits = 0;
            int i = startIndex;
            if (i < endIndex)
            {
                suits++;
                i += GetRunDown(i);
            }
            while (i < endIndex)
            {
                if (array[i - 1].Face - 1 != array[i].Face)
                {
                    // Found an out of sequence run in the range.
                    return -1;
                }
                suits++;
                i += GetRunDown(i);
            }
            return suits;
        }
    }
}
