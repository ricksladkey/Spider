using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Spider.Collections;
using Spider.Engine;

namespace Spider.GamePlay
{
    public class Game : Core, IGame
    {
        public static double[] FourSuitCoefficients = new double[] {
            /* 0 */ 4.97385808, 63.53977337, -0.07690241043, -3.361553585, -0.2933748314, 1.781253839, 4.819874539, 0.4819874538, 86.27048442,
            /* 9 */ 4.465708423, 0.001610653073, -0.1302184743, -0.9577011316, 2.95155848, 0.7840526817,
        };

        public static double[] TwoSuitCoefficients = new double[] {
            /* 0 */ 5.633744758, 80.97892108, -0.05372285251, -3.999455611, -0.9077026719, 0.8480919033, 9.447113329, 1, 76.38970958,
            /* 9 */ 4.191362497, 4.048432827E-05, -0.03960051729, -0.1601725542, 0.7790220167, 0.4819874539,
        };

        public static double[] OneSuitCoefficients = new double[] {
            /* 0 */ 4.241634919, 93.31341988, -0.08091391227, -3.265541832, -0.5942021654, 2.565712243, 17.64117551, 1, 110.0314895,
            /* 9 */ 1.756489081, 0.0002561898898, -0.04347481483, -0.1737026135, 3.471266012, 1,
        };

        public static double[] SearchCoefficients = new double[] {
            5, 1000, 2, 24
        };

        public const int MaximumMoves = 1500;

        public Variation Variation { get; set; }
        public int Seed { get; set; }
        public double[] Coefficients { get; set; }
        public bool TraceMoves { get; set; }
        public bool TraceStartFinish { get; set; }
        public bool TraceDeals { get; set; }
        public bool TraceSearch { get; set; }
        public bool ComplexMoves { get; set; }
        public bool Diagnostics { get; set; }
        public bool Interactive { get; set; }
        public int Instance { get; set; }
        public bool UseSearch { get; set; }

        public bool Won { get; private set; }

        public Pile Shuffled { get; private set; }
        public Tableau Tableau { get; private set; }
        public Tableau FindTableau { get; private set; }
        public IAlgorithm Algorithm { get; private set; }

        public MoveList Candidates { get; private set; }
        public MoveList SupplementaryMoves { get; private set; }
        public MoveList SupplementaryList { get; private set; }
        public HoldingStack[] HoldingStacks { get; private set; }
        public RunFinder RunFinder { get; private set; }
        public PileList OneRunPiles { get; private set; }
        public PileList[] FaceLists { get; private set; }
        public MoveList UncoveringMoves { get; private set; }
        public Tableau LastGame { get; private set; }
        public int NumberOfPiles { get; private set; }
        public int NumberOfSuits { get; private set; }

        private TableauInputOutput TableauInputOutput { get; set; }
        private BasicMoveFinder BasicMoveFinder { get; set; }
        private SwapMoveFinder SwapMoveFinder { get; set; }
        private CompositeSinglePileMoveFinder CompositeSinglePileMoveFinder { get; set; }
        private SearchMoveFinder SearchMoveFinder { get; set; }
        private MoveProcessor MoveProcessor { get; set; }

        public List<ComplexMove> ComplexCandidates
        {
            get
            {
                List<ComplexMove> result = new List<ComplexMove>();
                for (int i = 0; i < Candidates.Count; i++)
                {
                    result.Add(new ComplexMove(i, Candidates, SupplementaryList));
                }
                return result;
            }
        }

        static Game()
        {
        }

        public Game()
        {
            Variation = Variation.Spider4;
            Seed = -1;
            TraceStartFinish = false;
            TraceDeals = false;
            TraceMoves = false;
            ComplexMoves = false;
            Diagnostics = false;
            Instance = -1;

            Shuffled = new Pile();
            Tableau = new Tableau();

            Candidates = new MoveList();
            SupplementaryMoves = new MoveList();
            SupplementaryList = new MoveList();
            RunFinder = new RunFinder();
            OneRunPiles = new PileList();
            FaceLists = new PileList[(int)Face.King + 2];
            for (int i = 0; i < FaceLists.Length; i++)
            {
                FaceLists[i] = new PileList();
            }
            UncoveringMoves = new MoveList();
            Coefficients = null;

            TableauInputOutput = new TableauInputOutput(Tableau);

            BasicMoveFinder = new BasicMoveFinder(this);
            SwapMoveFinder = new SwapMoveFinder(this);
            CompositeSinglePileMoveFinder = new CompositeSinglePileMoveFinder(this);
            SearchMoveFinder = new SearchMoveFinder(this);
            MoveProcessor = new MoveProcessor(this);
        }

        public Game(Variation variation)
            : this()
        {
            Variation = variation;
        }

        public Game(string s)
            : this()
        {
            FromAsciiString(s);
        }

        public Game(Game other)
            : this()
        {
            FromGame(other);
        }

        public Game(Tableau tableau)
            : this()
        {
            FromTableau(tableau);
        }

        public void Initialize()
        {
            Tableau.Variation = Variation;
            NumberOfPiles = Variation.NumberOfPiles;
            NumberOfSuits = Variation.NumberOfSuits;
            if (UseSearch)
            {
                Algorithm = new SearchAlgorithm(this);
            }
            else
            {
                Algorithm = new StudyAlgorithm(this);
            }
            HoldingStacks = new HoldingStack[NumberOfPiles];
            for (int column = 0; column < NumberOfPiles; column++)
            {
                HoldingStacks[column] = new HoldingStack();
            }
            Won = false;
            Shuffled.Clear();
            Tableau.Clear();

            if (UseSearch)
            {
                SetDefaultCoefficients(SearchCoefficients);
            }
            else
            {
                int suits = Variation.NumberOfSuits;
                if (suits == 1)
                {
                    SetDefaultCoefficients(OneSuitCoefficients);
                }
                else if (suits == 2)
                {
                    SetDefaultCoefficients(TwoSuitCoefficients);
                }
                else if (suits == 4)
                {
                    SetDefaultCoefficients(FourSuitCoefficients);
                }
                else
                {
                    throw new Exception("invalid number of suits");
                }
            }
            LastGame = Debugger.IsAttached ? new Tableau(Tableau) : null;
        }

        public void Play()
        {
            try
            {
                Initialize();
                Algorithm.PrepareToPlay();
                Start();
                if (TraceStartFinish)
                {
                    PrintGame();
                }
                while (true)
                {
                    if (Interactive)
                    {
                        Console.Clear();
                        PrintGames();
                        Console.ReadKey();
                    }
                    if (Tableau.Moves.Count >= MaximumMoves)
                    {
                        if (TraceStartFinish)
                        {
                            PrintGame();
                            Utils.WriteLine("maximum moves exceeded");
                        }
                        throw new Exception("maximum moves exceeded");
                    }
                    CopyGame();
                    if (!MakeMove())
                    {
                        if (Tableau.StockPile.Count > 0)
                        {
                            Algorithm.PrepareToDeal();
                            if (TraceDeals)
                            {
                                PrintGame();
                                Utils.WriteLine("dealing");
                            }
                            Tableau.Deal();
                            Algorithm.RespondToDeal();
                            continue;
                        }
                        if (TraceStartFinish)
                        {
                            PrintGame();
                            Utils.WriteLine("lost - no moves");
                        }
                        break;
                    }
                    if (Won)
                    {
                        if (TraceStartFinish)
                        {
                            PrintGame();
                            Utils.WriteLine("won");
                        }
                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                Utils.WriteLine("spider: seed: {0}, message: {1}", Seed, exception.Message);
                throw;
            }
        }

        private void PrepareToPlay()
        {
        }

        public void Start()
        {
            if (Seed == -1)
            {
                Random random = new Random();
                Seed = random.Next();
            }
            Shuffled.Copy(Variation.Deck);
            Shuffled.Shuffle(Seed);
            Tableau.Layout(Shuffled);
        }

        private void SetDefaultCoefficients(double[] coefficients)
        {
            if (Coefficients == null)
            {
                Coefficients = new List<double>(coefficients).ToArray();
            }
        }

        private void CopyGame()
        {
            if (LastGame != null)
            {
                LastGame.Copy(Tableau);
            }
        }

        public bool MakeMove()
        {
            if (Tableau.NumberOfSpaces == NumberOfPiles)
            {
                Won = true;
                return true;
            }

            int checkPoint = Tableau.CheckPoint;
            Algorithm.MakeMove();
            return Tableau.CheckPoint != checkPoint;
        }

        public void FindMoves(Tableau tableau)
        {
            FindTableau = tableau;

            Analyze();

            if (!UseSearch)
            {
                int maxExtraSuits = ExtraSuits(FindTableau.NumberOfSpaces);
                FindUncoveringMoves(maxExtraSuits);
                FindOneRunPiles();
            }

            BasicMoveFinder.Find();
            SwapMoveFinder.Find();
            if (!UseSearch)
            {
                CompositeSinglePileMoveFinder.Find();
            }
        }

        private void FindUncoveringMoves(int maxExtraSuits)
        {
            // Find all uncovering moves.
            UncoveringMoves.Clear();
            HoldingStack holdingStack = new HoldingStack();
            for (int from = 0; from < NumberOfPiles; from++)
            {
                Pile fromPile = FindTableau[from];
                int fromRow = fromPile.Count - RunFinder.GetRunLengthAnySuit(from);
                if (fromRow == 0)
                {
                    continue;
                }
                int fromSuits = RunFinder.CountSuits(from, fromRow);
                Card fromCard = fromPile[fromRow];
                PileList faceList = FaceLists[(int)fromCard.Face + 1];
                for (int i = 0; i < faceList.Count; i++)
                {
                    holdingStack.Clear();
                    int to = faceList[i];
                    if (fromSuits - 1 > maxExtraSuits)
                    {
                        int holdingSuits = FindHolding(FindTableau, holdingStack, false, fromPile, from, fromRow, fromPile.Count, to, maxExtraSuits);
                        if (fromSuits - 1 > maxExtraSuits + holdingSuits)
                        {
                            break;
                        }
                    }
                    Pile toPile = FindTableau[to];
                    Card toCard = toPile[toPile.Count - 1];
                    int order = GetOrder(toCard, fromCard);
                    UncoveringMoves.Add(new Move(from, fromRow, to, order, AddHolding(holdingStack.Set)));
                }
            }
        }

        private void FindOneRunPiles()
        {
            OneRunPiles.Clear();
            for (int column = 0; column < NumberOfPiles; column++)
            {
                int upCount = FindTableau[column].Count;
                if (upCount != 0 && upCount == RunFinder.GetRunLengthAnySuit(column))
                {
                    OneRunPiles.Add(column);
                }
            }
        }

        public void SearchMoves()
        {
            SearchMoveFinder.SearchMoves();
        }

        public int AddSupplementary()
        {
            if (SupplementaryMoves.Count == 0)
            {
                return -1;
            }
            int first = SupplementaryList.Count;
            int count = SupplementaryMoves.Count;
            for (int i = 0; i < count; i++)
            {
                Move move = SupplementaryMoves[i];
                move.Next = i < count - 1 ? first + i + 1 : -1;
                SupplementaryList.Add(move);
            }
            return first;
        }

        public int AddHolding(HoldingSet holdingSet)
        {
            if (holdingSet.Count == 0)
            {
                return -1;
            }
            int first = SupplementaryList.Count;
            for (int i = 0; i < holdingSet.Count; i++)
            {
                HoldingInfo holding = holdingSet[i];
                int holdingNext = i < holdingSet.Count - 1 ? SupplementaryList.Count + 1 : -1;
                SupplementaryList.Add(new Move(holding.From, holding.FromRow, holding.To, holding.Length, -1, holdingNext));
            }
            return first;
        }

        public int AddHolding(HoldingSet holdingSet1, HoldingSet holdingSet2)
        {
            if (holdingSet1.Count == 0 && holdingSet2.Count == 0)
            {
                return -1;
            }
            if (holdingSet1.Count == 0)
            {
                return AddHolding(holdingSet2);
            }
            if (holdingSet2.Count == 0)
            {
                return AddHolding(holdingSet1);
            }
            int first1 = AddHolding(holdingSet1);
            int first2 = AddHolding(holdingSet2);
            int last1 = first1 + holdingSet1.Count - 1;
            Move holdingMove = SupplementaryList[last1];
            holdingMove.Next = first2;
            SupplementaryList[last1] = holdingMove;
            return first1;
        }

        public int FindHolding(IGetCard map, HoldingStack holdingStack, bool inclusive, Pile fromPile, int from, int fromStart, int fromEnd, int to, int maxExtraSuits)
        {
            holdingStack.StartingRow = fromEnd;
            int firstRow = fromStart + (inclusive ? 0 : 1);
            int lastRow = fromEnd - fromPile.GetRunUp(fromEnd);
            int extraSuits = 0;
            for (int fromRow = lastRow; fromRow >= firstRow; fromRow--)
            {
                if (fromRow < lastRow &&
                    fromPile[fromRow].Suit != fromPile[fromRow + 1].Suit)
                {
                    extraSuits++;
                }
                if (extraSuits > maxExtraSuits)
                {
                    return holdingStack.Suits;
                }
                Card fromCard = fromPile[fromRow];
                for (int column = 0; column < NumberOfPiles; column++)
                {
                    if (column == from || column == to)
                    {
                        continue;
                    }
                    if (fromCard.IsSourceFor(map.GetCard(column)))
                    {
                        int holdingSuits = extraSuits;
                        if (fromRow == fromStart || fromCard.Suit != fromPile[fromRow - 1].Suit)
                        {
                            holdingSuits++;
                        }
                        if (holdingSuits > holdingStack.Suits)
                        {
                            int length = holdingStack.FromRow - fromRow;
                            holdingStack.Push(new HoldingInfo(from, fromRow, column, holdingSuits, length));
                        }
                    }
                }
            }
            return holdingStack.Suits;
        }

        private void Analyze()
        {
            RunFinder.Find(FindTableau);

            // Prepare face lists.
            for (int i = (int)Face.Ace; i <= (int)Face.King; i++)
            {
                FaceLists[i].Clear();
            }
            for (int i = 0; i < NumberOfPiles; i++)
            {
                Pile pile = FindTableau[i];
                int pileCount = pile.Count;
                if (pileCount != 0 && !pile[pileCount - 1].IsEmpty)
                {
                    FaceLists[(int)pile[pileCount - 1].Face].Add(i);
                }
            }
        }

        public bool IsReversible(Move move)
        {
            int from = move.From;
            int fromRow = move.FromRow;
            int to = move.To;
            int toRow = move.ToRow;
            Pile fromPile = FindTableau[from];
            Pile toPile = FindTableau[to];
            bool isSwap = move.Type == MoveType.Swap;
            Card fromParent = fromRow != 0 ? fromPile[fromRow - 1] : Card.Empty;
            Card fromChild = fromPile[fromRow];
            Card toParent = toRow != 0 ? toPile[toRow - 1] : Card.Empty;
            Card toChild = toRow != toPile.Count ? toPile[toRow] : Card.Empty;
            int oldOrderFrom = GetOrder(fromParent, fromChild);
            int newOrderFrom = GetOrder(toParent, fromChild);
            int oldOrderTo = isSwap ? GetOrder(toParent, toChild) : 0;
            int newOrderTo = isSwap ? GetOrder(fromParent, toChild) : 0;
            return oldOrderFrom != 0 && (!isSwap || oldOrderTo != 0);
        }

        public bool IsViable(Move move)
        {
            int from = move.From;
            int fromRow = move.FromRow;
            int to = move.To;
            int toRow = move.ToRow;

            Pile fromPile = FindTableau[from];
            Pile toPile = FindTableau[to];
            if (toPile.Count == 0)
            {
                if (fromPile.Count == 0 && FindTableau.GetDownCount(from) == 0)
                {
                    return false;
                }
                else if (fromRow != 0 && fromPile[fromRow - 1].IsTargetFor(fromPile[fromRow]))
                {
                    return false;
                }
                return true;
            }
            bool isSwap = move.Type == MoveType.Swap;
            Card fromParent = fromRow != 0 ? fromPile[fromRow - 1] : Card.Empty;
            Card fromChild = fromPile[fromRow];
            Card toParent = toRow != 0 ? toPile[toRow - 1] : Card.Empty;
            Card toChild = toRow != toPile.Count ? toPile[toRow] : Card.Empty;
            int oldOrderFrom = GetOrder(fromParent, fromChild);
            int newOrderFrom = GetOrder(toParent, fromChild);
            int oldOrderTo = isSwap ? GetOrder(toParent, toChild) : 0;
            int newOrderTo = isSwap ? GetOrder(fromParent, toChild) : 0;
            int order = newOrderFrom - oldOrderFrom + newOrderTo - oldOrderTo;
            if (order < 0)
            {
                return false;
            }
            int netRunLengthFrom = RunFinder.GetNetRunLength(newOrderFrom, from, fromRow, to, toRow);
            int netRunLengthTo = isSwap ? RunFinder.GetNetRunLength(newOrderTo, to, toRow, from, fromRow) : 0;
            int netRunLength = netRunLengthFrom + netRunLengthTo;
            if (order == 0 && netRunLength < 0)
            {
                return false;
            }
            int delta = 0;
            if (order == 0 && netRunLength == 0)
            {
                if (!isSwap && oldOrderFrom == 1 && newOrderFrom == 1)
                {
                    delta = RunFinder.GetRunDelta(from, fromRow, to, toRow);
                }
                if (delta <= 0)
                {
                    return false;
                }
            }

            return true;
        }

        public void ProcessMove(Move move)
        {
            MoveProcessor.ProcessMove(move);
        }

        public void PrintGame()
        {
            PrintGame(this);
        }

        public static void PrintGame(Game game)
        {
            if (game == null)
            {
                return;
            }
            Utils.ColorizeToConsole(game.ToString());
        }

        public void PrintGames()
        {
            if (LastGame == null)
            {
                PrintGame();
                return;
            }
            PrintGamesSideBySide(LastGame, Tableau);
        }

        public void PrintMoves()
        {
            PrintMoves(Tableau.Moves);
        }

        public void PrintMoves(MoveList moves)
        {
            foreach (Move move in moves)
            {
                PrintMove(move);
            }
        }

        public void PrintCandidates()
        {
            foreach (Move move in Candidates)
            {
                PrintMove(move);
            }
        }

        public void PrintViableCandidates()
        {
            foreach (Move move in Candidates)
            {
                if (move.Score != Move.RejectScore)
                {
                    PrintMove(move);
                }
            }
        }

        public void PrintMove(Move move)
        {
            Utils.WriteLine(move);
            for (int next = move.Next; next != -1; next = SupplementaryList[next].Next)
            {
                Move nextMove = SupplementaryList[next];
                Utils.WriteLine("    {0}", nextMove);
            }
            for (int holdingNext = move.HoldingNext; holdingNext != -1; holdingNext = SupplementaryList[holdingNext].Next)
            {
                Utils.WriteLine("    holding {0}", SupplementaryList[holdingNext]);
            }
        }

        public string ToAsciiString()
        {
            return TableauInputOutput.ToAsciiString();
        }

        public void FromAsciiString(string s)
        {
            Initialize();
            TableauInputOutput.FromAsciiString(s);
            PrepareToPlay();
        }

        public void FromGame(Game other)
        {
            Initialize();
            TableauInputOutput.FromTableau(other.Tableau);
            PrepareToPlay();
        }

        public void FromTableau(Tableau tableau)
        {
            Initialize();
            TableauInputOutput.FromTableau(tableau);
            PrepareToPlay();
        }

        public string ToPrettyString()
        {
            return TableauInputOutput.ToPrettyString();
        }

        public static void PrintGamesSideBySide(Game game1, Game game2)
        {
            TableauInputOutput.PrintGamesSideBySide(game1.Tableau, game2.Tableau);
        }

        public static void PrintGamesSideBySide(Tableau game1, Tableau game2)
        {
            TableauInputOutput.PrintGamesSideBySide(game1, game2);
        }

        public override string ToString()
        {
            return TableauInputOutput.ToPrettyString();
        }
    }
}
