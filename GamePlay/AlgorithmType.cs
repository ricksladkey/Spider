using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider.GamePlay
{
    public struct AlgorithmType : IEquatable<AlgorithmType>
    {
        private enum Value
        {
            Empty = 0,
            Study = 1,
            Search = 2,
        }

        public static AlgorithmType Empty = new AlgorithmType(Value.Empty);
        public static AlgorithmType Study = new AlgorithmType(Value.Study);
        public static AlgorithmType Search = new AlgorithmType(Value.Search);

        public static bool operator==(AlgorithmType a, AlgorithmType b)
        {
            return a.Equals(b);
        }

        public static bool operator!=(AlgorithmType a, AlgorithmType b)
        {
            return !a.Equals(b);
        }

        public static AlgorithmType Parse(string s)
        {
            return new AlgorithmType((Value)Enum.Parse(typeof(Value), s, true));
        }

        private Value value;

        private AlgorithmType(Value value)
        {
            this.value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is AlgorithmType && Equals((AlgorithmType)obj);
        }

        public override int GetHashCode()
        {
            return (int)value;
        }

        #region IEquatable<AlgorithmType> Members

        public bool Equals(AlgorithmType other)
        {
            return value == other.value;
        }

        #endregion
    }
}
