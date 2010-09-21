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

        public void Update(int index, Pile pile)
        {
            Update(index, pile, pile.Count);
        }

        public void Update(int index, Pile pile, int count)
        {
            if (count == 0)
            {
                array[index] = Card.Empty;
            }
            else
            {
                array[index] = pile[count - 1];
            }
        }

        #region IGetCard Members

        public Card GetCard(int index)
        {
            return array[index];
        }

        #endregion
    }
}
