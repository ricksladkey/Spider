using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using NUnit.Framework;

using Spider;

namespace UnitTests
{
    [TestFixture]
    public class Tests
    {
        private Game game = null;

        [Test]
        public void InstantiationTest()
        {
            game = new Game();
        }

        [Test]
        public void SerializationTest()
        {
            string data1 = @"
                @2||KHTH3S5H9S-AH-5HKSAS7SKS-JH-7SKS8S8H-9HJH--6S3HQH-7S9S8H
                JH-9S3HJH4S|7H6H5H4H3H2HAH-2S-7H6S5S4S-KHQSJSTS9S-2SAS-TH-JS
                TS-2H-KH-2S|9HTSAS9H3SQSJS5STH4S8S3SQH9H8H2S5HQSAHTH3S4S5S2H
                8SAH7H6H6S4H4H8HQH5SQSTSAS7SKH2H6HKS8S4HQHJS6S3H6H7H@
            ";
            Game game1 = new Game(data1);
            Game game2 = new Game(game1.ToAsciiString());
            string data2 = game2.ToAsciiString();
            Assert.IsTrue(Strip(data1) == Strip(data2));
        }

        [Test]
        public void EmptyTest1()
        {
            // No cards: we win.
            string data = "@2||---------|---------|@";
            game = new Game(data);
            Assert.IsTrue(game.Move());
        }

        [Test]
        public void EmptyTest2()
        {
            // No useful move: we lose.
            string data = "@2||---------|AS---------|@";
            game = new Game(data);
            Assert.False(game.Move());
        }

        [Test]
        public void BuriedTest1()
        {
            // A simple buried move available with one free cell.
            string data = "@2||---------|4S8S-5S--KS-KS-KS-KS-KS-KS-KS|@";
            game = new Game(data);
            Assert.IsTrue(game.Move());
        }

        [Test]
        public void BuriedTest2()
        {
            // A simple buried move available but no free cells.
            string data = "@2||---------|4S8S-5S-KS-KS-KS-KS-KS-KS-KS-KS|@";
            game = new Game(data);
            Assert.IsFalse(game.Move());
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

        private void PrintGame()
        {
            Trace.WriteLine(game.ToString());
        }
    }
}
