using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    [DebuggerDisplay("Count = {count}")]
    [DebuggerTypeProxy(typeof(EnumerableDebugView))]
    public class CardMap : FastList<Card>, IGetCard
    {
        public CardMap()
            : base(Game.NumberOfPiles, Game.NumberOfPiles)
        {
        }

        public void Update(PileMap pileMap)
        {
            for (int pile = 0; pile < Count; pile++)
            {
                Pile other = pileMap[pile];
                Update(pile, other, other.Count);
            }
        }

        public void Update(int pile, Pile other)
        {
            Update(pile, other, other.Count);
        }

        public void Update(int pile, Pile other, int count)
        {
            if (count == 0)
            {
                array[pile] = Card.Empty;
            }
            else
            {
                array[pile] = other[count - 1];
            }
        }

        #region IGetCard Members

        public Card GetCard(int pile)
        {
            return array[pile];
        }

        #endregion
    }
}
