using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public struct Variation
    {
        private enum Value
        {
            Empty = 0,
            Spider1 = 1,
            Spider2 = 2,
            Spider4 = 4,
        }

        public static Variation Empty = new Variation(Value.Empty);
        public static Variation Spider1 = new Variation(Value.Spider1);
        public static Variation Spider2 = new Variation(Value.Spider2);
        public static Variation Spider4 = new Variation(Value.Spider4);

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

        public int NumberOfPiles
        {
            get
            {
                return 10;
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
                }
                throw new Exception("unknown variation");
            }
        }

        public string ToAsciiString()
        {
            switch (value)
            {
                case Variation.Value.Spider1: return "1";
                case Variation.Value.Spider2: return "2";
                case Variation.Value.Spider4: return "4";
            }
            throw new Exception("unknown variation");
        }

        public static Variation ParseAsciiString(string text)
        {
            if (text == "1") { return Variation.Spider1; }
            if (text == "2") { return Variation.Spider2; }
            if (text == "4") { return Variation.Spider4; }
            throw new Exception("unknown variation");
        }
    }

    public static class VariationExtensions
    {
    }
}
