using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Spider.Engine.Collections;

namespace Spider.Engine.Core
{
    [DebuggerDisplay("Count = {count}")]
    [DebuggerTypeProxy(typeof(EnumerableDebugView))]
    public class CardMap : FastList<Card>, IGetCard
    {
        public int NumberOfPiles
        {
            get
            {
                return count;
            }
            set
            {
                if (value != count)
                {
                    Clear();
                    for (int i = 0; i < value; i++)
                    {
                        Add(Card.Empty);
                    }
                }
            }
        }

        public CardMap()
            : base(10)
        {
        }

        public void Update(Tableau tableau)
        {
            Update(tableau.UpPiles);
        }

        public void Update(IList<Pile> pileMap)
        {
            NumberOfPiles = pileMap.Count;
            for (int column = 0; column < count; column++)
            {
                Pile pile = pileMap[column];
                Update(column, pile);
            }
        }

        public void Update(int column, Pile pile)
        {
            if (pile.Count == 0)
            {
                array[column] = Card.Empty;
            }
            else
            {
                array[column] = pile[pile.Count - 1];
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
