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
        public const int MaximumMoves = 1500;

        static Game()
        {
        }

        public Game()
        {
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
            SupplementaryList = new MoveList();
            RunFinder = new RunFinder();
            FaceLists = new PileList[(int)Face.King + 2];
            for (int i = 0; i < FaceLists.Length; i++)
            {
                FaceLists[i] = new PileList();
            }
            Coefficients = null;

            TableauInputOutput = new TableauInputOutput(Tableau);
            MoveProcessor = new MoveProcessor(this);

            Variation = Variation.Spider4;
            AlgorithmType = AlgorithmType.Study;
        }

        public Game(Variation variation)
            : this()
        {
            Variation = variation;
            Initialize();
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

        public Game(Variation variation, AlgorithmType algorithmType)
            : this()
        {
            Variation = variation;
            AlgorithmType = algorithmType;
            Initialize();
        }

        public Game(string s, AlgorithmType algorithmType)
            : this()
        {
            AlgorithmType = algorithmType;
            FromAsciiString(s);
        }

        public Game(Game other, AlgorithmType algorithmType)
            : this()
        {
            AlgorithmType = algorithmType;
            FromGame(other);
        }

        public Game(Tableau tableau, AlgorithmType algorithmType)
            : this()
        {
            AlgorithmType = algorithmType;
            FromTableau(tableau);
        }

        private Variation variation;
        private AlgorithmType algorithmType;
        private double[] coefficients;

        public Variation Variation
        {
            get { return variation; }
            set
            {
                if (variation != value)
                {
                    variation = value;
                    SetVariation();
                }
            }
        }
        public AlgorithmType AlgorithmType
        {
            get { return algorithmType; }
            set
            {
                if (algorithmType != value)
                {
                    algorithmType = value;
                    SetAlgorithm();
                }
            }
        }
        public double[] Coefficients
        {
            get { return coefficients; }
            set
            {
                if (!ArrayEquals(coefficients, value))
                {
                    coefficients = value;
                    SetCoefficients();
                }
            }
        }

        public int Seed { get; set; }
        public bool TraceMoves { get; set; }
        public bool TraceStartFinish { get; set; }
        public bool TraceDeals { get; set; }
        public bool TraceSearch { get; set; }
        public bool ComplexMoves { get; set; }
        public bool Diagnostics { get; set; }
        public bool Interactive { get; set; }
        public int Instance { get; set; }

        public bool Won { get; private set; }
        public bool Started { get; private set; }

        public Pile Shuffled { get; private set; }
        public Tableau Tableau { get; private set; }
        public Tableau FindTableau { get; private set; }
        public IAlgorithm Algorithm { get; private set; }

        public MoveList Candidates { get; private set; }
        public MoveList SupplementaryList { get; private set; }
        public HoldingStack[] HoldingStacks { get; private set; }
        public RunFinder RunFinder { get; private set; }
        public PileList[] FaceLists { get; private set; }
        public Tableau LastGame { get; private set; }
        public int NumberOfPiles { get; private set; }
        public int NumberOfSuits { get; private set; }

        private TableauInputOutput TableauInputOutput { get; set; }
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

        private void SetVariation()
        {
            Tableau.Variation = Variation;
            NumberOfPiles = Variation.NumberOfPiles;
            NumberOfSuits = Variation.NumberOfSuits;
            HoldingStacks = new HoldingStack[NumberOfPiles];
            for (int column = 0; column < NumberOfPiles; column++)
            {
                HoldingStacks[column] = new HoldingStack();
            }
        }

        private void SetAlgorithm()
        {
            Algorithm = GetAlgorithm();
        }

        private void SetCoefficients()
        {
            Algorithm.SetCoefficients();
        }

        public void Clear()
        {
            Started = false;
            Won = false;
            Shuffled.Clear();
            Tableau.Clear();
        }

        private void Initialize()
        {
            Clear();
            if (Algorithm == null)
            {
                Algorithm = GetAlgorithm();
            }
            Algorithm.Initialize();
            if (coefficients == null)
            {
                coefficients = Algorithm.GetCoefficients().ToArray();
            }
            LastGame = Debugger.IsAttached ? new Tableau(Tableau) : null;
        }

        private IAlgorithm GetAlgorithm()
        {
            if (AlgorithmType == AlgorithmType.Study)
            {
                return new StudyAlgorithm(this);
            }
            else if (AlgorithmType == AlgorithmType.Search)
            {
                return new SearchAlgorithm(this);
            }
            throw new Exception("unsupported algorithm type");
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
                    Print();
                }
                while (true)
                {
                    if (Interactive)
                    {
                        Console.Clear();
                        PrintBeforeAndAfter();
                        Console.ReadKey();
                    }
                    if (Tableau.Moves.Count >= MaximumMoves)
                    {
                        if (TraceStartFinish)
                        {
                            Print();
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
                                Print();
                                Utils.WriteLine("dealing");
                            }
                            Tableau.Deal();
                            Algorithm.RespondToDeal();
                            continue;
                        }
                        if (TraceStartFinish)
                        {
                            Print();
                            Utils.WriteLine("lost - no moves");
                        }
                        break;
                    }
                    if (Won)
                    {
                        if (TraceStartFinish)
                        {
                            Print();
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

        public void Start()
        {
            if (Seed == -1)
            {
                Random random = new Random();
                Seed = random.Next();
            }
            Shuffled.Copy(Variation.Deck);
            Shuffled.Shuffle(Seed);
            Tableau.PrepareLayout(Shuffled);
            Started = true;
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

        public void PrepareToFindMoves(Tableau tableau)
        {
            FindTableau = tableau;
            Analyze();
        }

        public int AddSupplementary(MoveList supplementaryMoves)
        {
            if (supplementaryMoves.Count == 0)
            {
                return -1;
            }
            int first = SupplementaryList.Count;
            int count = supplementaryMoves.Count;
            for (int i = 0; i < count; i++)
            {
                Move move = supplementaryMoves[i];
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

        public void ProcessMove(Move move)
        {
            MoveProcessor.Process(move);
        }

        public void Print()
        {
            Print(this);
        }

        public static void Print(Game game)
        {
            if (game == null)
            {
                return;
            }
            Utils.ColorizeToConsole(game.ToString());
        }

        public void PrintBeforeAndAfter()
        {
            if (LastGame == null)
            {
                Print();
                return;
            }
            PrintSideBySide(LastGame, Tableau);
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
            Variation = Tableau.Variation;
            Algorithm.PrepareToPlay();
        }

        public void FromGame(Game other)
        {
            Initialize();
            TableauInputOutput.FromTableau(other.Tableau);
            Variation = Tableau.Variation;
            Algorithm.PrepareToPlay();
        }

        public void FromTableau(Tableau tableau)
        {
            Initialize();
            TableauInputOutput.FromTableau(tableau);
            Variation = Tableau.Variation;
            Algorithm.PrepareToPlay();
        }

        public string ToPrettyString()
        {
            return TableauInputOutput.ToPrettyString();
        }

        public static void PrintSideBySide(Game game1, Game game2)
        {
            Utils.PrintSideBySide(game1.Tableau, game2.Tableau);
        }

        public static void PrintSideBySide(Tableau game1, Tableau game2)
        {
            Utils.PrintSideBySide(game1, game2);
        }

        public override string ToString()
        {
            return TableauInputOutput.ToPrettyString();
        }

        private static bool ArrayEquals<T>(T[] a, T[] b)
        {
            if (a == null && b == null)
            {
                return true;
            }
            if (a == null || b == null)
            {
                return false;
            }
            return (a as System.Collections.IStructuralEquatable).Equals(b, EqualityComparer<T>.Default);
        }
    }
}
