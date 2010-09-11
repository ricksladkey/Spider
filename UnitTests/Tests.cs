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
            Assert.AreEqual(Strip(data2), Strip(data1));
        }

        [Test]
        public void EmptyTest1()
        {
            // No cards: we win.
            string data = "@2||||@";
            game = new Game(data);
            Assert.IsTrue(game.Move());
        }

        [Test]
        public void EmptyTest2()
        {
            // No useful move: we lose.
            string data = "@2|||AS|@";
            game = new Game(data);
            Assert.False(game.Move());
        }

        [Test]
        public void BuriedTest1()
        {
            // A simple buried move available with one free cell.
            string data1 = "@2|||4S8S-5S--KS-KS-KS-KS-KS-KS-KS|@";
            string data2 = "@2|||8S-5S4S--KS-KS-KS-KS-KS-KS-KS|@";
            CheckMove(data1, data2);
        }

        [Test]
        public void BuriedTest2()
        {
            // A simple inversion move available with one free cell.
            string data1 = "@2|||4S5S-8S--KS-KS-KS-KS-KS-KS-KS|@";
            string data2 = "@2|||-8S-5S4S-KS-KS-KS-KS-KS-KS-KS|@";
            CheckMove(data1, data2);
        }

        [Test]
        public void BuriedTest3()
        {
            // A triple buried move available with one free cell.
            string data1 = "@2|||AS3S2S6S-4S-2S--KS-KS-KS-KS-KS-KS|@";
            string data2 = "@2|||6S-4S3S2S-2SAS--KS-KS-KS-KS-KS-KS|@";
            CheckMove(data1, data2);
        }

        [Test]
        public void BuriedTest4()
        {
            // A triple inversion move available with one free cell.
            string data1 = "@2|||AS2S3S--KS-KS-KS-KS-KS-KS-KS-KS|@";
            string data2 = "@2|||-3S2SAS-KS-KS-KS-KS-KS-KS-KS-KS|@";
            CheckMove(data1, data2);
        }

        [Test]
        public void BuriedTest5()
        {
            // A triple mixed buried move available with one free cell.
            string data1 = "@2|||4S2S3S-5S--KS-KS-KS-KS-KS-KS-KS|@";
            string data2 = "@2|||-5S4S-3S2S-KS-KS-KS-KS-KS-KS-KS|@";
            CheckMove(data1, data2);
        }

        [Test]
        public void BuriedTest6()
        {
            // A triple inversion move available with one free cell
            // with two holding cells.
            string data1 = "@2|||2S3S4S3H2S-4H3H-5S4S--KS-KS-KS-KS-KS-KS|@";
            string data2 = "@2|||-4H3H2S-5S4S3H-4S3S2S-KS-KS-KS-KS-KS-KS|@";
            CheckMove(data1, data2);
        }

        private void CheckMoveSucceeds(string data1, string data2)
        {
#if false
            PrintGame(new Game(data1));
            PrintGame(new Game(data2));
#endif
            // Check that the only available move is made.
            game = new Game(data1);
            Assert.IsTrue(game.Move());
            Assert.AreEqual(data2, game.ToAsciiString());
        }

        private void CheckMoveFails(string data)
        {
            // Check that the move is not made.
            game = new Game(data);
            Assert.IsFalse(game.Move());
            Assert.AreEqual(data, game.ToAsciiString());
        }

        private void CheckMove(string data1, string data2)
        {
            // Check that the only available move is made.
            CheckMoveSucceeds(data1, data2);

            // Check that the move is not made with one fewer free cell.
            CheckMoveFails(FillFreeCell(data1));
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

        private string FillFreeCell(string data)
        {
            return data.Replace("--", "-KS-");
        }

        private void PrintGame()
        {
            PrintGame(game);
        }

        private void PrintGame(Game game)
        {
            Trace.WriteLine(game.ToString());
        }
    }
}
