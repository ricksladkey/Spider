using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public struct Card : IEquatable<Card>
    {
        public static Card Empty = new Card();

        public Face Face { get; set; }
        public Suit Suit { get; set; }

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

        public bool IsSourceFor(Card other)
        {
            return Face.IsSourceFor(other.Face);
        }

        public bool IsTargetFor(Card other)
        {
            return Face.IsTargetFor(other.Face);
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

        public override int GetHashCode()
        {
            return (int)Face * (int)Suit;
        }

        #region IEquatable<Card> Members

        public bool Equals(Card other)
        {
            return Face == other.Face && Suit == other.Suit;
        }

        #endregion
    }
}
