using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Spider.Collections;

namespace Spider.Engine
{
    public struct Card : IEquatable<Card>
    {
        public static bool operator ==(Card lhs, Card rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Card lhs, Card rhs)
        {
            return !lhs.Equals(rhs);
        }

        public static Card Empty = new Card();

        public static Card Parse(string s)
        {
            return new Card(FaceUtils.Parse(s.Substring(0, 1)), SuitUtils.Parse(s.Substring(1, 1)));
        }

        public Card(Face face, Suit suit)
            : this()
        {
            Face = face;
            Suit = suit;
        }

        public Face Face;
        public Suit Suit;

        public bool IsEmpty { get { return Face == Face.Empty; } }

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

        public int HashKey
        {
            get
            {
                return (int)Face + (int)Suit;
            }
        }

        public string ToPrettyString()
        {
            return Face.ToAsciiString() + Suit.ToPrettyString();
        }

        public string ToAsciiString()
        {
            return Face.ToAsciiString() + Suit.ToAsciiString();
        }

        public override string ToString()
        {
            return ToPrettyString();
        }

        public override bool Equals(object obj)
        {
            return obj is Card && Equals((Card)obj);
        }

        public override int GetHashCode()
        {
            return HashKey;
        }

        #region IEquatable<Card> Members

        public bool Equals(Card other)
        {
            return Face == other.Face && Suit == other.Suit;
        }

        #endregion
    }
}
