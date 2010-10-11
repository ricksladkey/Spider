using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider.Tests
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DebugTestMethodAttribute : Attribute
    {
        public DebugTestMethodAttribute()
        {
        }
    }
}
