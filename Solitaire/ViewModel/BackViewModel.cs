using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spider.Engine;

namespace Spider.Solitaire.ViewModel
{
    public class BackViewModel
    {
        public BackViewModel(Card card)
        {
            Card = card;
        }

        public Card Card { get; private set; }
    }
}
