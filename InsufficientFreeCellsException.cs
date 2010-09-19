using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    class InsufficientFreeCellsException : Exception
    {
        public InsufficientFreeCellsException(string message)
            : base(message)
        {
        }
    }
}
