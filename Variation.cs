using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public enum Variation
    {
        Empty = 0,
        Spider1 = 1,
        Spider2 = 2,
        Spider4 = 4,
    }

    public static class VariationExtensions
    {
        public static int GetNumberOfPiles(this Variation variation)
        {
            return 10;
        }

        public static int GetNumberOfSuits(this Variation variation)
        {
            switch (variation)
            {
                case Variation.Spider1: return 1;
                case Variation.Spider2: return 2;
                case Variation.Spider4: return 4;
            }
            throw new Exception("unknown variation");
        }
    }
}
