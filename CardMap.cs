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
            for (int column = 0; column < Count; column++)
            {
                Pile pile = pileMap[column];
                Update(column, pile, pile.Count);
            }
        }

        public void Update(int column, Pile pile)
        {
            Update(column, pile, pile.Count);
        }

        public void Update(int column, Pile pile, int count)
        {
            if (count == 0)
            {
                array[column] = Card.Empty;
            }
            else
            {
                array[column] = pile[count - 1];
            }
        }

        #region IGetCard Members

        public Card GetCard(int column)
        {
            return array[column];
        }

        #endregion
    }
}
