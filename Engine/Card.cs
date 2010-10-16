using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Spider.Collections;

namespace Spider.Engine
{
    public struct Card : IEquatable<Card>
    {
        public static Card Empty = new Card();
        public static Card Unknown = new Card(Face.Unknown, Suit.Unknown);

        public Face Face;
        public Suit Suit;

        public bool IsEmpty { get { return Face == Face.Empty; } }

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

        public int GetHashKey()
        {
            return (int)Face + 14 * (int)Suit;
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
