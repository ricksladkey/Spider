using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Spider.Engine.Collections;

namespace Spider.Engine.Core
{
    public class Deck : Pile
    {
        public Deck()
            : this(1, 4)
        {
        }

        public Deck(int numberOfDecks)
            : this(numberOfDecks, 4)
        {
        }

        public Deck(int numberOfDecks, int numberOfSuits)
        {
            int reps = 0;
            if (numberOfSuits == 1)
            {
                reps = 4;
            }
            else if (numberOfSuits == 2)
            {
                reps = 2;
            }
            else if (numberOfSuits == 4)
            {
                reps = 1;
            }
            Suit[] suits = { Suit.Spades, Suit.Hearts, Suit.Clubs, Suit.Diamonds };
            for (int i = 0; i < numberOfDecks; i++)
            {
                for (Face face = Face.Ace; face <= Face.King; face++)
                {
                    for (int j = 0; j < numberOfSuits; j++)
                    {
                        Suit suit = suits[j];
                        for (int k = 0; k < reps; k++)
                        {
                            Add(new Card(face, suit));
                        }
                    }
                }
            }
        }
    }
}
