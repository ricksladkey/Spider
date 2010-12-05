using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spider.Engine;
using System.Windows.Input;

namespace Spider.Solitaire.ViewModel
{
    public class CardViewModel : IEquatable<CardViewModel>
    {
        public Card Card { get; set; }
        public int Column { get; set; }
        public int Row { get; set; }
        public bool IsSelectable { get; set; }

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
                return Card.Suit.GetColor() == SuitColor.Red ? "#FFDD0000" : "Black";
            }
        }

        public override string ToString()
        {
            return string.Format("Card: {0}, Column = {1}, Row = {2}, IsSelectable = {3}", Card, Column, Row, IsSelectable);
        }

        #region IEquatable<CardViewModel> Members

        public bool Equals(CardViewModel other)
        {
            return Card == other.Card && Column == other.Column && Row == other.Row && IsSelectable == other.IsSelectable;
        }

        #endregion
    }
}
