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

        public int Suits { get { return game.Suits; } }
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
        public List<HoldingInfo> HoldingList { get { return game.HoldingList; } }

        public GameHelper(Game game)
        {
            this.game = game;
        }

        public Move Normalize(Move move)
        {
            return game.Normalize(move);
        }
    }
}
