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
        public CardType CardType { get; set; }
        public int Column { get; set; }
        public int Row { get; set; }
        public bool IsMoveSelectable { get; set; }
        public bool IsAutoSelectable { get; set; }

        public bool IsSelectable { get { return IsMoveSelectable || IsAutoSelectable; } }
        public string Face { get { return Card.Face.ToLabel(); } }
        public string Suit { get { return Card.Suit.ToPrettyString(); } }
        public SuitColor Color { get { return (SuitColor)Card.Suit.GetColor(); } }
        public string Name { get { return Card.ToAsciiString(); } }

        public override string ToString()
        {
            return string.Format("Card={0}, CardType={1}, Column={2}, Row={3}, IsMoveSelectable={4}, IsAutoSelectable={5}",
                Card, CardType, Column, Row, IsMoveSelectable, IsAutoSelectable);
        }

        #region IEquatable<CardViewModel> Members

        public bool Equals(CardViewModel other)
        {
            return
                Card == other.Card &&
                Column == other.Column &&
                Row == other.Row &&
                IsMoveSelectable == other.IsMoveSelectable &&
                IsAutoSelectable == other.IsAutoSelectable;
        }

        #endregion
    }
}
