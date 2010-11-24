using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spider.Engine;

namespace Spider.Solitaire.ViewModel
{
    public class CardViewModel
    {
        public CardViewModel(Card card)
        {
            Card = card;
        }

        public Card Card { get; set; }

        public string Face
        {
            get
            {
                return Card.Face.ToLabel();
            }
        }

        public string Suit
        {
            get
            {
                return Card.Suit.ToPrettyString();
            }
        }

        public string Color
        {
            get
            {
                return Card.Suit.GetColor() == SuitColor.Red ? "DarkRed" : "Black";
            }
        }
    }
}
