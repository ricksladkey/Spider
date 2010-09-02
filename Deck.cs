using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public class Deck : Pile
    {
        public Deck()
            : this(1, 4)
        {
        }

        public Deck(int decks)
            : this(decks, 4)
        {
        }

        public Deck(int decks, int suits)
        {
            int reps = 0;
            if (suits == 1)
            {
                reps = 4;
            }
            else if (suits == 2)
            {
                reps = 2;
            }
            else if (suits == 4)
            {
                reps = 1;
            }
            for (int i = 0; i < decks; i++)
            {
                for (Face face = Face.Ace; face <= Face.King; face++)
                {
                    for (Suit suit = (Suit)1; suit <= (Suit)suits; suit++)
                    {
                        for (int j = 0; j < reps; j++)
                        {
                            Add(new Card(face, suit));
                        }
                    }
                }
            }
        }
    }
}
