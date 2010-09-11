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
                @2||KhTh3s5h9s-Ah-5hKsAs7sKs-Jh-7sKs8s8h-9hJh--6s3hQh-7s9s8h
                Jh-9s3hJh4s|7h6h5h4h3h2hAh-2s-7h6s5s4s-KhQsJsTs9s-2sAs-Th-Js
                Ts-2h-Kh-2s|9hTsAs9h3sQsJs5sTh4s8s3sQh9h8h2s5hQsAhTh3s4s5s2h
                8sAh7h6h6s4h4h8hQh5sQsTsAs7sKh2h6hKs8s4hQhJs6s3h6h7h@
            ";
            Game game1 = new Game(data1);
            Game game2 = new Game(game1.ToAsciiString());
            string data2 = game2.ToAsciiString();
            Assert.AreEqual(Normalize(data2), Normalize(data1));
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
        public void SwapTest1()
        {
            // A 1/1 swap move, 1 free cell.
            string data1 = "@2|||9s8h-9h8s--Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||9s8s-9h8h--Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [Test]
        public void SwapTest2()
        {
            // A 1/1 whole swap move, 1 free cell.
            string data1 = "@2|||8h-9h8s--Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||8s-9h8h--Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [Test]
        public void SwapTest3()
        {
            // A 1/3 swap move, 2 free cells.
            string data1 = "@2|||9s8h-9h8s7s6s5h---Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||9s8s7s6s5h-9h8h---Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [Test]
        public void SwapTest4()
        {
            // A 2/2 swap move, 2 free cells.
            string data1 = "@2|||Ts4h3s-5h9s8h---Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||Ts9s8h-5h4h3s---Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [Test]
        public void SwapTest5()
        {
            // A 2/2 whole pile swap move, 2 free cells.
            string data1 = "@2|||4h3s-5h9s8h---Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||9s8h-5h4h3s---Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [Test]
        public void SwapTest6()
        {
            // A 1/6 swap move, 3 free cells.
            string data1 = "@2|||Ts4h-5h9s8h7s6h5s4h----Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||Ts9s8h7s6h5s4h-5h4h----Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [Test]
        public void SwapTest7()
        {
            // A 2/5 swap move, 3 free cells.
            string data1 = "@2|||Js4h3s-5hTs9h8s7h6s----Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||JsTs9h8s7h6s-5h4h3s----Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [Test]
        public void SwapTest8()
        {
            // A 3/4 swap move, 3 free cells.
            string data1 = "@2|||Js4h3s2h-5hTs9h8s7h----Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||JsTs9h8s7h-5h4h3s2h----Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [Test]
        public void OffloadTest1()
        {
            // A 1/1 offload move, 1 free cell.
            string data1 = "@2|||4s8s-5s--Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||8s-5s4s--Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [Test]
        public void OffloadTest2()
        {
            // A 1/1 inversion move, 1 free cell.
            string data1 = "@2|||4s5s-8s--Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||-8s-5s4s-Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [Test]
        public void OffloadTest3()
        {
            // A 1/1/1 offload move, 1 free cell.
            string data1 = "@2|||As3s2s6s-4s-2s--Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||6s-4s3s2s-2sAs--Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [Test]
        public void OffloadTest4()
        {
            // A 1/1/1 inversion move, 1 free cell.
            string data1 = "@2|||As2s3s--Ks-Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||-3s2sAs-Ks-Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [Test]
        public void OffloadTest5()
        {
            // A 1/1/1 mixed offload move, 1 free cell.
            string data1 = "@2|||4s2s3s-5s--Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||-5s4s-3s2s-Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [Test]
        public void OffloadTest6()
        {
            // A 1/1/1 inversion move, 1 free cell
            // with one holding cell.
            string data1 = "@2|||2s3s4s3s2h-5h4h--Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||-5h4h3s2h-4s3s2s-Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [Test]
        public void OffloadTest7()
        {
            // A 1/1/1 inversion move, 1 free cell
            // with two holding cells.
            string data1 = "@2|||2s3s4s3h2s-4h3h-5s4s--Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||-4h3h2s-5s4s3h-4s3s2s-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [Test]
        public void OffloadTest8()
        {
            // A 1/2 offload move, 2 free cells.
            string data1 = "@2|||As8s7h-2s---Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||8s7h-2sAs---Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [Test]
        public void OffloadTest9()
        {
            // A 1/3 offload move, 2 free cells.
            string data1 = "@2|||As8s7h6s-2s---Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||8s7h6s-2sAs---Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [Test]
        public void OffloadTest10()
        {
            // A 2/2 offload move, 2 free cells.
            string data1 = "@2|||Ts9h7h6s-Js---Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||7h6s-JsTs9h---Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [Test]
        public void OffloadTest11()
        {
            // A 1/4 offload move, three free cells.
            string data1 = "@2|||Ts8s7h6s5h-Js----Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||8s7h6s5h-JsTs----Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [Test]
        public void OffloadTest12()
        {
            // A 1/6 offload move, three free cells.
            string data1 = "@2|||Ts8s7h6s5h4s3h-Js----Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||8s7h6s5h4s3h-JsTs----Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [Test]
        public void OffloadTest13()
        {
            // A 2/5 offload move, three free cells.
            string data1 = "@2|||9s8h8s7h6s5h4s-Ts----Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||8s7h6s5h4s-Ts9s8h----Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [Test]
        public void OffloadTest14()
        {
            // A 4/3 offload move, three free cells.
            string data1 = "@2|||Ts9h8s7h8s7h6s-Js----Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||8s7h6s-JsTs9h8s7h----Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        private void CheckMoveSucceeds(string initial, string expected)
        {
            // Check that the only available move is made.
            game = new Game(initial);
            Assert.IsTrue(game.Move());
            string actual = game.ToAsciiString();
            AreEqual(expected, actual);
        }

        private static void AreEqual(string expected, string actual)
        {
            if (expected != actual)
            {
                Utils.WriteLine("expected: {0}", expected);
                Utils.WriteLine("actual:   {0}", actual);
            }
            Assert.AreEqual(expected, actual);
        }

        private void CheckMoveFails(string initial)
        {
            // Check that the move is not made
            // or that a last resort move was made.
            game = new Game(initial);
            int before = game.EmptyFreeCells;
            if (game.Move())
            {
                int after = game.EmptyFreeCells;
                Assert.IsTrue(after < before);
            }
            else
            {
                string actual = game.ToAsciiString();
                AreEqual(initial, actual);
            }
        }

        private void CheckMove(string initial, string expected)
        {
            // Check that the only available move is made.
            CheckMoveSucceeds(initial, expected);

            // Check that the move is not made with one fewer free cell.
            CheckMoveFails(FillFreeCell(initial));
        }

        private string Normalize(string s)
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
            return b.ToString().ToUpperInvariant();
        }

        private string FillFreeCell(string data)
        {
            return data.Replace("--", "-Ks-");
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
