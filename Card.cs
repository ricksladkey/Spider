using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public struct Card : IEquatable<Card>
    {
        public static Card Empty = new Card();
        public static Card Unknown = new Card(Face.Unknown, Suit.Unknown);

        public Face Face { get; set; }
        public Suit Suit { get; set; }

        public bool IsEmpty { get { return Face == Face.Empty; } }
        public bool IsUnknown { get { return Face == Face.Unknown; } }

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
            if (IsEmpty)
            {
                return 0;
            }
            if (IsUnknown)
            {
                return 53;
            }
            return (int)Face + 13 * ((int)Suit - 1);
        }

        #region IEquatable<Card> Members

        public bool Equals(Card other)
        {
            return Face == other.Face && Suit == other.Suit;
        }

        #endregion
    }
}
