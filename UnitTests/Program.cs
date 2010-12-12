using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Spider.Collections;
using Spider.Engine;
using Spider.Tests;

namespace UnitTests
{
    public static class Program
    {
        static void Main(string[] args)
        {
            RunTests(typeof(Tests), args);
        }

        static void RunTests(Type type, string[] tests)
        {
            int count = 0;
            object[] args = new object[0];
            ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);
            object obj = ctor.Invoke(args);
            foreach (MethodInfo methodInfo in GetMethods(type, tests))
            {
                try
                {
                    methodInfo.Invoke(obj, args);
                }
                catch (TargetInvocationException)
                {
                }
                count++;
            }
            Utils.WriteLine("Class: {0}, tests: {1}", type.Name, count);
        }

        static List<MethodInfo> GetMethods(Type type, string[] tests)
        {
            List<MethodInfo> list = new List<MethodInfo>();
            MethodInfo[] methods = type.GetMethods();
            foreach (MethodInfo methodInfo in methods)
            {
                if (tests.Length > 0 && !tests.Contains(methodInfo.Name))
                {
                    continue;
                }
                object[] attributeArray = methodInfo.GetCustomAttributes(typeof(DebugTestMethodAttribute), false);
                foreach (DebugTestMethodAttribute attribute in attributeArray)
                {
                    list.Add(methodInfo);
                    break;
                }
            }
            return list;
        }
    }
}
