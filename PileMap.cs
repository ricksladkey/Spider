using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    [DebuggerDisplay("Count = {count}")]
    [DebuggerTypeProxy(typeof(EnumerableDebugView))]
    public class PileMap : FastList<Pile>, IGetCard
    {
        public PileMap()
            : base(Game.NumberOfPiles, Game.NumberOfPiles)
        {
            for (int index = 0; index < Game.NumberOfPiles; index++)
            {
                array[index] = new Pile();
            }
        }

        public void ClearAll()
        {
            for (int i = 0; i < Game.NumberOfPiles; i++)
            {
                array[i].Clear();
            }
        }

        #region IGetCard Members

        public Card GetCard(int index)
        {
            Pile pile = array[index];
            int count = pile.Count;
            return count == 0 ? Card.Empty : pile[count - 1];
        }

        #endregion
    }
}
