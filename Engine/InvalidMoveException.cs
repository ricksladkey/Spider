using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Spider.Collections;

namespace Spider.Engine
{
    [Serializable]
    public class InvalidMoveException : Exception
    {
        public InvalidMoveException(string message)
            : base(message)
        {
        }
    }
}
