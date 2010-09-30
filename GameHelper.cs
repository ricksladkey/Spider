using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public class GameHelper : BaseGame
    {
        protected Game game;

        public static int NumberOfPiles { get { return Game.NumberOfPiles; } }

        public int Suits { get { return game.Suits; } set { game.Suits = value; } }
        public int Seed { get { return game.Seed; } }
        public double[] Coefficients { get { return game.Coefficients; } }
        public bool Diagnostics { get { return game.Diagnostics; } }

        public bool TraceMoves { get { return game.TraceMoves; } }
        public bool ComplexMoves { get { return game.ComplexMoves; } }
        public bool RecordComplex { get { return game.RecordComplex; } }

        public MoveList Moves { get { return game.Moves; } }

        public Pile Deck { get { return game.Deck; } }
        public Pile Shuffled { get { return game.Shuffled; } }
        public Pile StockPile { get { return game.StockPile; } }
        public PileMap UpPiles { get { return game.UpPiles; } }
        public PileMap DownPiles { get { return game.DownPiles; } }
        public List<Pile> DiscardPiles { get { return game.DiscardPiles; } }

        public MoveList UncoveringMoves { get { return game.UncoveringMoves; } }
        public MoveList SupplementaryMoves { get { return game.SupplementaryMoves; } }
        public MoveList SupplementaryList { get { return game.SupplementaryList; } }
        public MoveList Candidates { get { return game.Candidates; } }
        public PileList EmptyPiles { get { return game.EmptyPiles; } }
        public PileList OneRunPiles { get { return game.OneRunPiles; } }
        public List<HoldingInfo> HoldingList { get { return game.HoldingList; } }

        public GameHelper(Game game)
        {
            this.game = game;
        }

        public void Initialize()
        {
            game.Initialize();
        }

        public int FindEmptyPiles()
        {
            return game.FindEmptyPiles();
        }

        public void Discard()
        {
            game.Discard();
        }

        public void TurnOverCards()
        {
            game.TurnOverCards();
        }

        public Move Normalize(Move move)
        {
            return game.Normalize(move);
        }

        public int AddSupplementary()
        {
            return game.AddSupplementary();
        }

        public int FindHolding(IGetCard map, HoldingStack holdingStack, bool inclusive, Pile fromPile, int from, int fromStart, int fromEnd, int to, int maxExtraSuits)
        {
            return game.FindHolding(map, holdingStack, inclusive, fromPile, from, fromStart, fromEnd, to, maxExtraSuits);
        }

        public void PrintGame()
        {
            game.PrintGame();
        }

        public void PrintGames()
        {
            game.PrintGames();
        }

        private void PrintMoves()
        {
            game.PrintMoves();
        }

        private void PrintMoves(MoveList moves)
        {
            game.PrintMoves(moves);
        }

        private void PrintCandidates()
        {
            game.PrintCandidates();
        }

        private void PrintViableCandidates()
        {
            game.PrintViableCandidates();
        }

        public void PrintMove(Move move)
        {
            game.PrintMove(move);
        }
    }
}
