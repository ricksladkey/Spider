using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public struct Card : IEquatable<Card>
    {
        public Face Face { get; set; }
        public Suit Suit { get; set; }

        public static Card Empty { get { return new Card(); } }

        public bool IsEmpty { get { return Equals(Empty); } }

        public Card(Face face, Suit suit)
            : this()
        {
            Face = face;
            Suit = suit;
        }

        public void Clear()
        {
            this = Card.Empty;
        }

        public string ToPrettyString()
        {
            return Utils.GetString(Face) + Utils.GetPrettyString(Suit);
        }

        public string ToAsciiString()
        {
            return Utils.GetString(Face) + Utils.GetAsciiString(Suit);
        }

        public override string ToString()
        {
            return ToPrettyString();
        }

        #region IEquatable<Card> Members

        public bool Equals(Card other)
        {
            return Face == other.Face && Suit == other.Suit;
        }

        #endregion
    }
}
