using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using NUnit.Framework;

using Spider;

namespace UnitTests
{
    public class Program
    {
        static void Main(string[] args)
        {
            RunTests(typeof(Tests));
        }

        static void RunTests(Type type)
        {
            int count = 0;
            ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);
            object[] args = new object[0];
            object obj = ctor.Invoke(args);
            MethodInfo[] methods = type.GetMethods();
            foreach (MethodInfo methodInfo in methods)
            {
                object[] attributeArray = methodInfo.GetCustomAttributes(typeof(TestAttribute), false);
                foreach (TestAttribute attribute in attributeArray)
                {
                    methodInfo.Invoke(obj, args);
                    count++;
                }
            }
            Utils.WriteLine("Class: {0}, tests: {1}", type.Name, count);
        }
    }
}
