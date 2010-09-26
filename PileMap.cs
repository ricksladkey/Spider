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

        public int GetRunDown(int pile, int index)
        {
            return array[pile].GetRunDown(index);
        }

        public int GetRunDownAnySuit(int pile, int index)
        {
            return array[pile].GetRunDownAnySuit(index);
        }

        public int GetRunUp(int pile, int index)
        {
            return array[pile].GetRunUp(index);
        }

        public int GetRunUpAnySuit(int pile, int index)
        {
            return array[pile].GetRunUpAnySuit(index);
        }

        public int CountSuits(int pile, int index)
        {
            return array[pile].CountSuits(index, -1);
        }

        public int CountSuits(int pile, int startIndex, int endIndex)
        {
            return array[pile].CountSuits(startIndex, endIndex);
        }

        public int GetRunDelta(int from, int fromIndex, int to, int toIndex)
        {
            return GetRunUp(from, fromIndex) - GetRunUp(to, toIndex);
        }

        #region IGetCard Members

        public Card GetCard(int pile)
        {
            return array[pile].LastCard;
        }

        #endregion
    }
}
