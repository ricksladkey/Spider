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
            AddRange(other);
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
            // Knuth shuffle algorithm: for each card
            // except the last, swap it with one of the
            // later cards.
            Random random = new Random(seed);
            for (int i = 0; i < Count - 1; i++)
            {
                int swap = random.Next(Count - i);
                Card tmp = array[i + swap];
                array[i + swap] = array[i];
                array[i] = tmp;
            }
        }

        public int GetRunUp(int row)
        {
            Debug.Assert(row >= 0 && row <= Count);
            if (row < 2)
            {
                return row;
            }
            int runLength = 1;
            int i = row - 2;
            Card card = array[i + 1];
            do
            {
                Card nextCard = array[i];
                if (nextCard.Suit != card.Suit)
                {
                    break;
                }
                if (!nextCard.IsTargetFor(card))
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

        public int GetRunUpAnySuit(int row)
        {
            Debug.Assert(row >= 0 && row <= Count);
            if (row < 2)
            {
                return row;
            }
            int runLength = 1;
            int i = row - 2;
            Face face = array[i + 1].Face;
            do
            {
                Face nextFace = array[i].Face;
                if (!nextFace.IsTargetFor(face))
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

        public int GetRunDown(int row)
        {
            Debug.Assert(row >= 0 && row <= Count);
            if (row > count - 2)
            {
                return count - row;
            }
            int runLength = 1;
            int i = row + 1;
            Card card = array[i - 1];
            do
            {
                Card nextCard = array[i];
                if (nextCard.Suit != card.Suit)
                {
                    break;
                }
                if (!nextCard.IsSourceFor(card))
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

        public int GetRunDownAnySuit(int row)
        {
            if (row > count - 2)
            {
                return count - row;
            }
            int runLength = 1;
            int i = row + 1;
            Face face = array[i - 1].Face;
            do
            {
                Face nextFace = array[i].Face;
                if (!nextFace.IsSourceFor(face))
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

        public int CountSuits()
        {
            return CountSuits(0, -1);
        }

        public int CountSuits(int row)
        {
            return CountSuits(row, -1);
        }

        public int CountSuits(int startRow, int endRow)
        {
            if (endRow == -1)
            {
                endRow = Count;
            }
            Debug.Assert(startRow >= 0 && startRow <= Count);
            Debug.Assert(endRow >= 0 && endRow <= Count);
            int suits = 0;
            int i = startRow;
            if (i < endRow)
            {
                suits++;
                i += GetRunDown(i);
            }
            while (i < endRow)
            {
                if (!array[i - 1].IsTargetFor(array[i]))
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
