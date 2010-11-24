using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spider.Engine;

namespace Spider.Solitaire.ViewModel
{
    public class SpiderViewModel
    {
        public SpiderViewModel()
        {
            AceOfHearts = new CardViewModel(new Card(Face.Ace, Suit.Hearts));
        }

        public CardViewModel AceOfHearts { get; private set; }
    }
}
