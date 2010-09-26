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
            Debug.Assert(index >= 0 && index <= Count);
            if (index < 2)
            {
                return index;
            }
            int runLength = 1;
            int i = index - 2;
            Card card = array[i + 1];
            do
            {
                Card nextCard = array[i];
                if (nextCard.Suit != card.Suit)
                {
                    break;
                }
                if (nextCard.Face - 1 != card.Face)
                {
                    break;
                }
                card = nextCard;
                runLength++;
                i--;
            }
            while (i >= 0);
            return runLength;
        }

        public int GetRunUpAnySuit(int index)
        {
            Debug.Assert(index >= 0 && index <= Count);
            if (index < 2)
            {
                return index;
            }
            int runLength = 1;
            int i = index - 2;
            Face face = array[i + 1].Face;
            do
            {
                Face nextFace = array[i].Face;
                if (nextFace - 1 != face)
                {
                    break;
                }
                face = nextFace;
                runLength++;
                i--;
            }
            while (i >= 0);
            return runLength;
        }

        public int GetRunDown(int index)
        {
            Debug.Assert(index >= 0 && index <= Count);
            if (index > count - 2)
            {
                return count - index;
            }
            int runLength = 1;
            int i = index + 1;
            Card card = array[i - 1];
            do
            {
                Card nextCard = array[i];
                if (nextCard.Suit != card.Suit)
                {
                    break;
                }
                if (nextCard.Face + 1 != card.Face)
                {
                    break;
                }
                card = nextCard;
                runLength++;
                i++;
            }
            while (i < count);
            return runLength;
        }

        public int GetRunDownAnySuit(int index)
        {
            if (index > count - 2)
            {
                return count - index;
            }
            int runLength = 1;
            int i = index + 1;
            Face face = array[i - 1].Face;
            do
            {
                Face nextFace = array[i].Face;
                if (nextFace + 1 != face)
                {
                    break;
                }
                face = nextFace;
                runLength++;
                i++;
            }
            while (i < count);
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
