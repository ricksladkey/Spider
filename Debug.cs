using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    public static class Debug
    {
        [Conditional("DEBUG")]
        public static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                else
                {
                    throw new Exception("assert failed");
                }
            }
        }

        [Conditional("DEBUG")]
        public static void Assert(bool condition)
        {
            Assert(condition, "assert failed");
        }

        [Conditional("DEBUG")]
        public static void Fail(string message)
        {
            Assert(false, message);
        }
    }
}
