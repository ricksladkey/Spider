using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Spider.Collections;

namespace Spider.Engine
{
    [DebuggerDisplay("{value}")]
    public struct Variation : IEquatable<Variation>
    {
        private enum Value
        {
            Empty = 0,
            Spider1 = 1,
            Spider2 = 2,
            Spider4 = 3,
            Spiderette1 = 4,
            Spiderette2 = 5,
            Spiderette4 = 6,
        }

        public static Variation Empty = new Variation(Value.Empty);
        public static Variation Spider1 = new Variation(Value.Spider1);
        public static Variation Spider2 = new Variation(Value.Spider2);
        public static Variation Spider4 = new Variation(Value.Spider4);
        public static Variation Spiderette1 = new Variation(Value.Spiderette1);
        public static Variation Spiderette2 = new Variation(Value.Spiderette2);
        public static Variation Spiderette4 = new Variation(Value.Spiderette4);

        public static bool operator==(Variation a, Variation b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Variation a, Variation b)
        {
            return !a.Equals(b);
        }

        private static Pile SpiderOneSuitDeck = new Deck(2, 1);
        private static Pile SpiderTwoSuitDeck = new Deck(2, 2);
        private static Pile SpiderFourSuitDeck = new Deck(2, 4);

        private static Pile SpideretteOneSuitDeck = new Deck(1, 1);
        private static Pile SpideretteTwoSuitDeck = new Deck(1, 2);
        private static Pile SpideretteFourSuitDeck = new Deck(1, 4);

        private static FastList<LayoutPart> SpiderLayoutParts = new FastList<LayoutPart>
        {
            new LayoutPart(0, 10),
            new LayoutPart(0, 10),
            new LayoutPart(0, 10),
            new LayoutPart(0, 10),
            new LayoutPart(0, 4),
        };

        private static FastList<LayoutPart> SpideretteLayoutParts = new FastList<LayoutPart>
        {
            new LayoutPart(1, 6),
            new LayoutPart(2, 5),
            new LayoutPart(3, 4),
            new LayoutPart(4, 3),
            new LayoutPart(5, 2),
            new LayoutPart(6, 1),
        };

        private Value value;

        private Variation(Value value)
            : this()
        {
            this.value = value;
        }

        public Variation(Variation other)
            : this()
        {
            value = other.value;
        }

        public int NumberOfDecks
        {
            get
            {
                switch (value)
                {
                    case Value.Spider1: return 2;
                    case Value.Spider2: return 2;
                    case Value.Spider4: return 2;
                    case Value.Spiderette1: return 1;
                    case Value.Spiderette2: return 1;
                    case Value.Spiderette4: return 1;
                }
                throw new InvalidOperationException("unknown variation");
            }
        }

        public int NumberOfCards
        {
            get
            {
                return NumberOfDecks * 52;
            }
        }

        public int NumberOfFaceDownCards
        {
            get
            {
                int total = 0;
                foreach (LayoutPart layoutPart in LayoutParts)
                {
                    total += layoutPart.Count;
                }
                return total;
            }
        }

        public int NumberOfFaceUpCards
        {
            get
            {
                return NumberOfPiles;
            }
        }

        public int NumberOfStockCards
        {
            get
            {
                return NumberOfCards - NumberOfFaceDownCards - NumberOfFaceUpCards;
            }
        }

        public int NumberOfFoundations
        {
            get
            {
                return NumberOfDecks * 4;
            }
        }

        public int NumberOfPiles
        {
            get
            {
                switch (value)
                {
                    case Value.Spider1: return 10;
                    case Value.Spider2: return 10;
                    case Value.Spider4: return 10;
                    case Value.Spiderette1: return 7;
                    case Value.Spiderette2: return 7;
                    case Value.Spiderette4: return 7;
                }
                throw new InvalidOperationException("unknown variation");
            }
        }

        public int NumberOfSuits
        {
            get
            {
                switch (value)
                {
                    case Value.Spider1: return 1;
                    case Value.Spider2: return 2;
                    case Value.Spider4: return 4;
                    case Value.Spiderette1: return 1;
                    case Value.Spiderette2: return 2;
                    case Value.Spiderette4: return 4;
                }
                throw new InvalidOperationException("unknown variation");
            }
        }

        public Pile Deck
        {
            get
            {
                switch (value)
                {
                    case Value.Spider1: return SpiderOneSuitDeck;
                    case Value.Spider2: return SpiderTwoSuitDeck;
                    case Value.Spider4: return SpiderFourSuitDeck;
                    case Value.Spiderette1: return SpideretteOneSuitDeck;
                    case Value.Spiderette2: return SpideretteTwoSuitDeck;
                    case Value.Spiderette4: return SpideretteFourSuitDeck;
                }
                throw new InvalidOperationException("unknown variation");
            }
        }

        public IList<LayoutPart> LayoutParts
        {
            get
            {
                switch (value)
                {
                    case Value.Spider1: return SpiderLayoutParts;
                    case Value.Spider2: return SpiderLayoutParts;
                    case Value.Spider4: return SpiderLayoutParts;
                    case Value.Spiderette1: return SpideretteLayoutParts;
                    case Value.Spiderette2: return SpideretteLayoutParts;
                    case Value.Spiderette4: return SpideretteLayoutParts;
                }
                throw new InvalidOperationException("unknown variation");
            }
        }

        public string ToAsciiString()
        {
            switch (value)
            {
                case Value.Spider1: return "1";
                case Value.Spider2: return "2";
                case Value.Spider4: return "4";
                case Value.Spiderette1: return "1t";
                case Value.Spiderette2: return "2t";
                case Value.Spiderette4: return "4t";
            }
            throw new InvalidOperationException("unknown variation");
        }

        public static Variation FromAsciiString(string text)
        {
            if (text == "1") { return Variation.Spider1; }
            if (text == "2") { return Variation.Spider2; }
            if (text == "4") { return Variation.Spider4; }
            if (text == "1t") { return Variation.Spiderette1; }
            if (text == "2t") { return Variation.Spiderette2; }
            if (text == "4t") { return Variation.Spiderette4; }
            throw new InvalidOperationException("unknown variation");
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is Variation)
            {
                return Equals((Variation)obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (int)value;
        }

        #region IEquatable<Variation> Members

        public bool Equals(Variation other)
        {
            return value == other.value;
        }

        #endregion
    }
}
