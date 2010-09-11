using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using NUnit.Framework;

namespace UnitTests
{
    public class Program
    {
        static void Main(string[] args)
        {
            Tests tests = new Tests();
            object[] noArgs = new object[0];
            MethodInfo[] methods = typeof(Tests).GetMethods();
            foreach (MethodInfo methodInfo in methods)
            {
                object[] attributeArray = methodInfo.GetCustomAttributes(typeof(TestAttribute), false);
                foreach (TestAttribute attribute in attributeArray)
                {
                    methodInfo.Invoke(tests, noArgs);
                }
            }
        }
    }
}
