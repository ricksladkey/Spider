using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using Spider;

namespace UnitTests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void InstantiationTest()
        {
            Game game = new Game();
        }

        [Test]
        public void SerializationTest()
        {
            string data1 = @"
                @|KHTH3S5H9S-AH-5HKSAS7SKS-JH-7SKS8S8H-9HJH--6S3HQH-7S9S8HJH
                -9S3HJH4S|7H6H5H4H3H2HAH-2S-7H6S5S4S-KHQSJSTS9S-2SAS-TH-JSTS
                -2H-KH-2S|9HTSAS9H3SQSJS5STH4S8S3SQH9H8H2S5HQSAHTH3S4S5S2H8S
                AH7H6H6S4H4H8HQH5SQSTSAS7SKH2H6HKS8S4HQHJS6S3H6H7H@
            ";
            Game game1 = new Game(data1);
            Game game2 = new Game(game1.ToAsciiString());
            string data2 = game2.ToAsciiString();
            Assert.IsTrue(Strip(data1) == Strip(data2));
        }

        private string Strip(string s)
        {
            StringBuilder b = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (!char.IsWhiteSpace(c))
                {
                    b.Append(c);
                }
            }
            return b.ToString();
        }
    }
}
