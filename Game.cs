using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    public class Game : BaseGame
    {
        public static double[] FourSuitCoefficients = new double[] {
            /* 0 */ 8.42581707, 42.35984891, -0.1201269187, -4.841970863, -0.1252077493, 4.908558385, 8.502830004, 1,
            /* 8 */ 2.241481832, 0.003208907113, -0.1594844085, -0.9196463991, 5.422359166, 1,
        };

        public static double[] TwoSuitCoefficients = new double[] {
            /* 0 */ 6.899899999, 68.85499999, -0.07135475932, -6.247428091, -0.836999815, 0.9974230967, 9.447113329, 0.6666666667,
            /* 8 */ 3.030276207, 9.10897386E-05, -0.03104889178, -0.1737026136, 1.122100782, 0.4819874539,
        };

        public static double[] OneSuitCoefficients = new double[] {
            /* 0 */ 4.241634919, 93.31341988, -0.08091391227, -3.265541832, -0.5942021654, 2.565712243, 17.64117551, 1,
            /* 8 */ 1.756489081, 0.0002561898898, -0.04347481483, -0.1737026135, 3.471266012, 1,
        };

        public const int NumberOfPiles = 10;
        public const int MaximumMoves = 1500;

        public const int Group0 = 0;
        public const int Group1 = 8;

        public const double InfiniteScore = double.MaxValue;
        public const double RejectScore = double.MinValue;

        public static Deck OneSuitDeck { get; private set; }
        public static Deck TwoSuitDeck { get; private set; }
        public static Deck FourSuitDeck { get; private set; }

        public int Suits { get; set; }
        public int Seed { get; set; }
        public double[] Coefficients { get; set; }
        public bool TraceStartFinish { get; set; }
        public bool TraceDeals { get; set; }
        public bool TraceMoves { get; set; }
        public bool ComplexMoves { get; set; }
        public bool RecordComplex { get; set; }
        public bool Diagnostics { get; set; }
        public int Instance { get; set; }

        public bool Won { get; private set; }
        public MoveList Moves { get; private set; }

        public Pile Deck { get; private set; }
        public Pile Shuffled { get; private set; }
        public Pile StockPile { get; private set; }
        public PileMap DownPiles { get; private set; }
        public PileMap UpPiles { get; private set; }
        public List<Pile> DiscardPiles { get; private set; }

        public MoveList Candidates { get; private set; }
        public MoveList SupplementaryMoves { get; private set; }
        public MoveList SupplementaryList { get; private set; }
        public HoldingStack HoldingStack { get; private set; }
        public List<HoldingInfo> HoldingList { get; private set; }
        public int[] RunLengths { get; private set; }
        public int[] RunLengthsAnySuit { get; private set; }
        public PileList EmptyPiles { get; private set; }
        public PileList OneRunPiles { get; private set; }
        public PileList[] FaceLists { get; private set; }
        public MoveList UncoveringMoves { get; private set; }
        public Game LastGame { get; private set; }

        private GameInputOutput GameInputOutput { get; set; }
        private CompositeSinglePileMoveFinder CompositeSinglePileMoveFinder { get; set; }
        private MoveProcessor MoveProcessor { get; set; }

        public List<ComplexMove> ComplexCandidates
        {
            get
            {
                List<ComplexMove> result = new List<ComplexMove>();
                for (int i = 0; i < Candidates.Count; i++)
                {
                    result.Add(new ComplexMove(i, Candidates, SupplementaryList, HoldingList));
                }
                return result;
            }
        }

        public int NumberOfEmptyPiles
        {
            get
            {
                return FindEmptyPiles();
            }
        }

        static Game()
        {
            OneSuitDeck = new Deck(2, 1);
            TwoSuitDeck = new Deck(2, 2);
            FourSuitDeck = new Deck(2, 4);
        }

        public Game()
        {
            Suits = 4;
            Seed = -1;
            TraceStartFinish = false;
            TraceDeals = false;
            TraceMoves = false;
            ComplexMoves = false;
            RecordComplex = false;
            Diagnostics = false;
            Instance = -1;

            Moves = new MoveList();
            Shuffled = new Pile();
            StockPile = new Pile();
            DownPiles = new PileMap();
            UpPiles = new PileMap();
            DiscardPiles = new List<Pile>();

            Candidates = new MoveList();
            SupplementaryMoves = new MoveList();
            SupplementaryList = new MoveList();
            HoldingStack = new HoldingStack();
            HoldingList = new List<HoldingInfo>();
            RunLengths = new int[NumberOfPiles];
            RunLengthsAnySuit = new int[NumberOfPiles];
            EmptyPiles = new PileList();
            OneRunPiles = new PileList();
            FaceLists = new PileList[(int)Face.King + 2];
            for (int i = 0; i < FaceLists.Length; i++)
            {
                FaceLists[i] = new PileList();
            }
            UncoveringMoves = new MoveList();
            Coefficients = null;

            GameInputOutput = new Spider.GameInputOutput(this);
            CompositeSinglePileMoveFinder = new CompositeSinglePileMoveFinder(this);
            MoveProcessor = new MoveProcessor(this);
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

        public void Play()
        {
            LastGame = Debugger.IsAttached ? new Game() : null;
            try
            {
                Initialize();
                Start();
                if (TraceStartFinish)
                {
                    PrintGame();
                }
                while (true)
                {
                    if (Moves.Count >= MaximumMoves)
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
                        if (StockPile.Count > 0)
                        {
                            PrepareToDeal();
                            if (TraceDeals)
                            {
                                PrintGame();
                                Utils.WriteLine("dealing");
                            }
                            Deal();
                            RespondToDeal();
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

        public void Initialize()
        {
            Won = false;
            Moves.Clear();
            Candidates.Clear();
            Shuffled.Clear();
            StockPile.Clear();
            DownPiles.ClearAll();
            UpPiles.ClearAll();
            DiscardPiles.Clear();

            if (Suits == 1)
            {
                SetDefaultCoefficients(OneSuitCoefficients);
                Deck = OneSuitDeck;
            }
            else if (Suits == 2)
            {
                SetDefaultCoefficients(TwoSuitCoefficients);
                Deck = TwoSuitDeck;
            }
            else if (Suits == 4)
            {
                SetDefaultCoefficients(FourSuitCoefficients);
                Deck = FourSuitDeck;
            }
            else
            {
                throw new Exception("Invalid number of suits");
            }
        }

        public void Start()
        {
            if (Seed == -1)
            {
                Random random = new Random();
                Seed = random.Next();
            }
            Shuffled.AddRange(Deck);
            Shuffled.Shuffle(Seed);
            StockPile.AddRange(Shuffled);

            int column = 0;
            for (int i = 0; i < 44; i++)
            {
                DownPiles[column].Add(StockPile.Next());
                column = (column + 1) % NumberOfPiles;
            }
            Deal();
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
                LastGame.FromGame(this);
            }
        }

        private void Deal()
        {
            for (int i = 0; i < NumberOfPiles; i++)
            {
                UpPiles[i].Add(StockPile.Next());
            }
        }

        public bool MakeMove()
        {
            Analyze();

            if (EmptyPiles.Count == NumberOfPiles)
            {
                Won = true;
                return true;
            }

            FindMoves();
            return ChooseMove();
        }

        public void FindMoves()
        {
            Candidates.Clear();
            SupplementaryList.Clear();
            HoldingList.Clear();

            int emptyPiles = EmptyPiles.Count;
            int maxExtraSuits = ExtraSuits(emptyPiles);
            int maxExtraSuitsToEmptyPile = ExtraSuits(emptyPiles - 1);

            FindUncoveringMoves(maxExtraSuits);
            FindOneRunPiles();

            for (int from = 0; from < NumberOfPiles; from++)
            {
                Pile fromPile = UpPiles[from];
                HoldingStack.Clear();
                HoldingStack.StartingRow = fromPile.Count;
                int extraSuits = 0;
                for (int fromRow = fromPile.Count - 1; fromRow >= 0; fromRow--)
                {
                    Card fromCard = fromPile[fromRow];
                    if (fromRow < fromPile.Count - 1)
                    {
                        Card previousCard = fromPile[fromRow + 1];
                        if (!previousCard.IsSourceFor(fromCard))
                        {
                            break;
                        }
                        if (fromCard.Suit != previousCard.Suit)
                        {
                            // This is a cross-suit run.
                            extraSuits++;
                            if (extraSuits > maxExtraSuits + HoldingStack.Suits)
                            {
                                break;
                            }
                        }
                    }

                    // Add moves to other piles.
                    if (fromCard.Face < Face.King)
                    {
                        PileList piles = FaceLists[(int)fromCard.Face + 1];
                        for (int i = 0; i < piles.Count; i++)
                        {
                            foreach (HoldingSet holdingSet in HoldingStack.Sets)
                            {
                                if (extraSuits > maxExtraSuits + holdingSet.Suits)
                                {
                                    continue;
                                }
                                int to = piles[i];
                                if (from == to || holdingSet.Contains(from))
                                {
                                    continue;
                                }

                                // We've found a legal move.
                                Pile toPile = UpPiles[to];
                                Candidates.Add(new Move(from, fromRow, to, toPile.Count, AddHolding(holdingSet)));

                                // Update the holding pile move.
                                int holdingSuits = extraSuits;
                                if (fromRow > 0 && (!fromPile[fromRow - 1].IsTargetFor(fromCard) || fromCard.Suit != fromPile[fromRow - 1].Suit))
                                {
                                    holdingSuits++;
                                }
                                if (holdingSuits > HoldingStack.Suits)
                                {
                                    int length = HoldingStack.FromRow - fromRow;
                                    HoldingStack.Push(new HoldingInfo(from, fromRow, to, holdingSuits, length));
                                }

                                break;
                            }
                        }
                    }

                    // Add moves to an empty pile.
                    for (int i = 0; i < EmptyPiles.Count; i++)
                    {
                        int to = EmptyPiles[0];

                        if (fromRow == 0)
                        {
                            // No point in moving from a full pile
                            // from one open position to another unless
                            // there are more cards to turn over.
                            if (DownPiles[from].Count == 0)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            // No point in moving anything less than
                            // as much as possible to an empty pile.
                            Card nextCard = fromPile[fromRow - 1];
                            if (fromCard.Suit == nextCard.Suit)
                            {
                                if (nextCard.IsTargetFor(fromCard))
                                {
                                    continue;
                                }
                            }
                        }

                        foreach (HoldingSet holdingSet in HoldingStack.Sets)
                        {
                            if (holdingSet.FromRow == fromRow)
                            {
                                // No cards left to move.
                                continue;
                            }
                            if (extraSuits > maxExtraSuitsToEmptyPile + holdingSet.Suits)
                            {
                                // Not enough empty piles.
                                continue;
                            }

                            // We've found a legal move.
                            Pile toPile = UpPiles[to];
                            Candidates.Add(new Move(from, fromRow, to, toPile.Count, AddHolding(holdingSet)));
                            break;
                        }

                        // Only need to check the first empty pile
                        // since all empty piles are the same
                        // except for undealt cards.
                        break;
                    }

                    // Check for swaps.
                    CheckSwaps(from, fromRow, extraSuits, maxExtraSuits);
                }

                // Check for composite single pile moves.
                CompositeSinglePileMoveFinder.Check(from);
            }
        }

        private void FindUncoveringMoves(int maxExtraSuits)
        {
            // Find all uncovering moves.
            UncoveringMoves.Clear();
            for (int from = 0; from < NumberOfPiles; from++)
            {
                Pile fromPile = UpPiles[from];
                int fromRow = fromPile.Count - RunLengthsAnySuit[from];
                if (fromRow == 0)
                {
                    continue;
                }
                int fromSuits = fromPile.CountSuits(fromRow);
                if (fromSuits - 1 > maxExtraSuits)
                {
                    continue;
                }
                Card fromCard = fromPile[fromRow];
                PileList faceList = FaceLists[(int)fromCard.Face + 1];
                for (int i = 0; i < faceList.Count; i++)
                {
                    int to = faceList[i];
                    Pile toPile = UpPiles[to];
                    Card toCard = toPile[toPile.Count - 1];
                    int order = GetOrder(toCard, fromCard);
                    UncoveringMoves.Add(new Move(from, fromRow, to, order));
                }
            }
        }

        private void FindOneRunPiles()
        {
            OneRunPiles.Clear();
            for (int i = 0; i < NumberOfPiles; i++)
            {
                int downCount = DownPiles[i].Count;
                int upCount = UpPiles[i].Count;
                if (downCount == 0 && upCount != 0 && upCount == RunLengthsAnySuit[i])
                {
                    OneRunPiles.Add(i);
                }
            }
        }

        private void PrepareToDeal()
        {
        }

        private void RespondToDeal()
        {
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

        private int AddHolding(HoldingSet holdingSet)
        {
            if (holdingSet.Count == 0)
            {
                return -1;
            }
            int first = HoldingList.Count;
            for (int i = 0; i < holdingSet.Count; i++)
            {
                HoldingInfo holding = holdingSet[i];
                holding.Next = i < holdingSet.Count - 1 ? HoldingList.Count + 1 : -1;
                HoldingList.Add(holding);
            }
            return first;
        }

        private int AddHolding(HoldingSet holdingSet1, HoldingSet holdingSet2)
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
            HoldingInfo holding = HoldingList[last1];
            holding.Next = first2;
            HoldingList[last1] = holding;
            return first1;
        }

        private void CheckSwaps(int from, int fromRow, int extraSuits, int maxExtraSuits)
        {
#if false
            if (extraSuits + 1 > maxExtraSuits + HoldingStack.Suits)
            {
                // Need at least one empty pile or a holding pile to swap.
                return;
            }
#endif
            if (fromRow == 0 && DownPiles[from].Count != 0)
            {
                // Would turn over a card.
                return;
            }
            Pile fromPile = UpPiles[from];
            Card fromCard = fromPile[fromRow];
            Card fromCardParent = Card.Empty;
            bool inSequence = true;
            if (fromRow != 0)
            {
                fromCardParent = fromPile[fromRow - 1];
                inSequence = fromCardParent.IsTargetFor(fromCard);
            }
            for (int to = 0; to < NumberOfPiles; to++)
            {
                Pile toPile = UpPiles[to];
                if (to == from || toPile.Count == 0)
                {
                    continue;
                }
                int splitRow = toPile.Count - RunLengthsAnySuit[to];
                int toRow = -1;
                if (inSequence)
                {
                    // Try to find from counterpart in the first to run.
                    toRow = splitRow + (int)(toPile[splitRow].Face - fromCard.Face);
                    if (toRow < splitRow || toRow >= toPile.Count)
                    {
                        // Sequence doesn't contain our counterpart.
                        continue;
                    }
                }
                else
                {
                    // Try to swap with both runs out of sequence.
                    toRow = splitRow;
                    if (fromRow != 0 && !fromCardParent.IsTargetFor(toPile[toRow]))
                    {
                        // Cards don't match.
                        continue;
                    }
                }
                if (toRow == 0)
                {
                    if (fromRow == 0)
                    {
                        // No point in swap both entire piles.
                        continue;
                    }
                    if (DownPiles[to].Count != 0)
                    {
                        // Would turn over a card.
                        continue;
                    }
                }
                else if (!toPile[toRow - 1].IsTargetFor(fromCard))
                {
                    // Cards don't match.
                    continue;
                }

#if false
                int toSuits = toPile.CountSuits(toRow);
                HoldingStack forwardHoldingStack = new HoldingStack();
                if (extraSuits + toSuits > maxExtraSuits)
                {
                    // Check whether forward holding piles will help.
                    int forwardHoldingSuits = FindHolding(UpPiles, forwardHoldingStack, true, from, fromRow, fromPile.Count, to, maxExtraSuits);
                    if (extraSuits + toSuits > maxExtraSuits + forwardHoldingSuits)
                    {
                        // Prepare an accurate map.
                        CardMap map = new CardMap();
                        map.Update(UpPiles);
                        foreach (HoldingInfo holding in forwardHoldingStack.Set)
                        {
                            map[holding.To] = fromPile[holding.FromRow + holding.Length - 1];
                        }

                        // Check whether reverse holding piles will help.
                        HoldingStack reverseHoldingStack = new HoldingStack();
                        int reverseHoldingSuits = FindHolding(map, reverseHoldingStack, true, to, toRow, toPile.Count, from, maxExtraSuits);
                        if (extraSuits + toSuits > maxExtraSuits + forwardHoldingSuits + reverseHoldingSuits)
                        {
                            continue;
                        }

                        Candidates.Add(new Move(MoveType.Swap, from, fromRow, to, toRow, AddHolding(forwardHoldingStack.Set, reverseHoldingStack.Set)));
                        continue;
                    }
                }

                // We've found a legal swap.
                Debug.Assert(toRow == 0 || toPile[toRow - 1].IsTargetFor(fromCard));
                Debug.Assert(fromRow == 0 || fromCardParent.IsTargetFor(toPile[toRow]));
                Candidates.Add(new Move(MoveType.Swap, from, fromRow, to, toRow, AddHolding(forwardHoldingStack.Set)));
#else
                int toSuits = toPile.CountSuits(toRow);
                bool foundSwap = false;
                foreach (HoldingSet holdingSet in HoldingStack.Sets)
                {
                    if (holdingSet.Contains(to))
                    {
                        // The pile is already in use.
                        continue;
                    }
                    if (extraSuits + toSuits > maxExtraSuits + holdingSet.Suits)
                    {
                        // Not enough empty piles.
                        continue;
                    }

                    // We've found a legal swap.
                    Debug.Assert(toRow == 0 || toPile[toRow - 1].IsTargetFor(fromCard));
                    Debug.Assert(fromRow == 0 || fromCardParent.IsTargetFor(toPile[toRow]));
                    Candidates.Add(new Move(MoveType.Swap, from, fromRow, to, toRow, AddHolding(holdingSet)));
                    foundSwap = true;
                    break;
                }

                if (!foundSwap)
                {
                    // Check whether reverse holding piles will help.
                    HoldingSet holdingSet = HoldingStack.Set;
                    if (!holdingSet.Contains(to))
                    {
                        // Prepare an accurate map.
                        CardMap map = new CardMap();
                        map.Update(UpPiles);
                        foreach (HoldingInfo holding in holdingSet)
                        {
                            map[holding.To] = fromPile[holding.FromRow + holding.Length - 1];
                        }

                        HoldingStack reverseHoldingStack = new HoldingStack();
                        int reverseHoldingSuits = FindHolding(map, reverseHoldingStack, true, toPile, to, toRow, toPile.Count, from, maxExtraSuits);
                        if (extraSuits + toSuits <= maxExtraSuits + holdingSet.Suits + reverseHoldingSuits)
                        {
                            Candidates.Add(new Move(MoveType.Swap, from, fromRow, to, toRow, AddHolding(holdingSet, reverseHoldingStack.Set)));
                        }
                    }
                }
#endif
            }
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

        public int FindEmptyPiles()
        {
            EmptyPiles.Clear();
            for (int i = 0; i < NumberOfPiles; i++)
            {
                if (UpPiles[i].Count == 0)
                {
                    EmptyPiles.Add(i);
                }
            }
            return EmptyPiles.Count;
        }

        private void Analyze()
        {
            EmptyPiles.Clear();
            for (int i = (int)Face.Ace; i <= (int)Face.King; i++)
            {
                FaceLists[i].Clear();
            }

            for (int i = 0; i < NumberOfPiles; i++)
            {
                // Prepare empty piles and face lists.
                Pile pile = UpPiles[i];
                if (pile.Count == 0)
                {
                    EmptyPiles.Add(i);
                }
                else
                {
                    FaceLists[(int)pile[pile.Count - 1].Face].Add(i);
                }

                // Cache run lengths.
                RunLengths[i] = pile.GetRunUp(pile.Count);
                RunLengthsAnySuit[i] = pile.GetRunUpAnySuit(pile.Count);
            }
        }

        private double CalculateScore(Move move)
        {
            if (move.IsEmpty)
            {
                return RejectScore;
            }

            ScoreInfo score = new ScoreInfo(Coefficients, Group0);

            int from = move.From;
            int fromRow = move.FromRow;
            int to = move.To;
            int toRow = move.ToRow;

            if (move.Type == MoveType.CompositeSinglePile)
            {
                return CalculateCompositeSinglePileScore(move);
            }
            Pile fromPile = UpPiles[from];
            Pile toPile = UpPiles[to];
            if (toPile.Count == 0)
            {
                return CalculateLastResortScore(move);
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
            score.Order = newOrderFrom - oldOrderFrom + newOrderTo - oldOrderTo;
            if (score.Order < 0)
            {
                return RejectScore;
            }
            score.Uses = CountUses(move);
            score.OneRunDelta = !isSwap ? GetOneRunDelta(oldOrderFrom, newOrderFrom, move) : 0;
            int faceFrom = (int)fromChild.Face;
            int faceTo = isSwap ? (int)toChild.Face : 0;
            score.FaceValue = Math.Max(faceFrom, faceTo);
            bool wholePile = fromRow == 0 && toRow == toPile.Count;
            int netRunLengthFrom = GetNetRunLength(newOrderFrom, from, fromRow, to, toRow);
            int netRunLengthTo = isSwap ? GetNetRunLength(newOrderTo, to, toRow, from, fromRow) : 0;
            score.NetRunLength = netRunLengthFrom + netRunLengthTo;
            score.DownCount = DownPiles[from].Count;
            score.TurnsOverCard = wholePile && score.DownCount != 0;
            score.CreatesEmptyPile = wholePile && score.DownCount == 0;
            score.NoEmptyPiles = EmptyPiles.Count == 0;
            if (score.Order == 0 && score.NetRunLength < 0)
            {
                return RejectScore;
            }
            int delta = 0;
            if (score.Order == 0 && score.NetRunLength == 0)
            {
                if (!isSwap && oldOrderFrom == 1 && newOrderFrom == 1)
                {
                    delta = UpPiles.GetRunDelta(from, fromRow, to, toRow);
                }
                if (delta <= 0)
                {
                    return RejectScore;
                }
            }
            score.IsCompositeSinglePile = false;

            return score.Score;
        }

        private double CalculateCompositeSinglePileScore(Move move)
        {
            ScoreInfo score = new ScoreInfo(Coefficients, Group0);

            score.Order = move.ToRow;
            score.FaceValue = 0;
            score.NetRunLength = 0;
            score.DownCount = DownPiles[move.From].Count;
            score.TurnsOverCard = move.Flags.TurnsOverCard();
            score.CreatesEmptyPile = move.Flags.CreatesEmptyPile();
            score.UsesEmptyPile = move.Flags.UsesEmptyPile();
            score.IsCompositeSinglePile = true;
            score.NoEmptyPiles = EmptyPiles.Count == 0;
            score.OneRunDelta = 0;

            if (score.UsesEmptyPile)
            {
                // XXX: should calculate uses, is king, etc.
                score.Coefficient0 = Group1;
                return score.LastResortScore;
            }
            return score.Score;
        }

        private double CalculateLastResortScore(Move move)
        {
            ScoreInfo score = new ScoreInfo(Coefficients, Group1);

            Pile fromPile = UpPiles[move.From];
            Pile toPile = UpPiles[move.To];
            Card fromCard = fromPile[move.FromRow];
            bool wholePile = move.FromRow == 0;
            score.UsesEmptyPile = true;
            score.DownCount = DownPiles[move.From].Count;
            score.TurnsOverCard = wholePile && score.DownCount != 0;
            score.FaceValue = (int)fromCard.Face;
            score.IsKing = fromCard.Face == Face.King;
            score.Uses = CountUses(move);

            if (wholePile)
            {
                // Only move an entire pile if there
                // are more cards to be turned over.
                if (!score.TurnsOverCard)
                {
                    return RejectScore;
                }
            }
            else if (fromPile[move.FromRow - 1].IsTargetFor(fromCard))
            {
                // No point in splitting consecutive cards
                // unless they are part of a multi-move
                // sequence.
                return RejectScore;
            }

            return score.LastResortScore;
        }

        private int GetOneRunDelta(int oldOrder, int newOrder, Move move)
        {
            bool fromFree = DownPiles[move.From].Count == 0;
            bool toFree = DownPiles[move.To].Count == 0;
            bool fromUpper = UpPiles.GetRunUp(move.From, move.FromRow) == move.FromRow;
            bool fromLower = move.HoldingNext == -1;
            bool toUpper = UpPiles.GetRunUp(move.To, move.ToRow) == move.ToRow;
            bool oldFrom = move.FromRow == 0 ?
                (fromFree && fromLower) :
                (fromFree && fromUpper && fromLower && oldOrder == 2);
            bool newFrom = fromFree && fromUpper;
            bool oldTo = toFree && toUpper;
            bool newTo = move.ToRow == 0 ?
                (toFree && fromLower) :
                (toFree && toUpper && fromLower && newOrder == 2);
            int oneRunDelta = (newFrom ? 1 : 0) - (oldFrom ? 1 : 0) + (newTo ? 1 : 0) - (oldTo ? 1 : 0);
#if false
            if (oneRunDelta != 0)
            {
                Console.Clear();
                PrintMove(move);
                PrintGame();
                Debugger.Break();
            }
#endif
            return oneRunDelta > 0 ? 1 : 0;
        }

        private int CountUses(Move move)
        {
            if (move.FromRow == 0 || move.ToRow != UpPiles[move.To].Count)
            {
                // No exposed card, no uses.
                return 0;
            }

            int uses = 0;

            Pile fromPile = UpPiles[move.From];
            Card fromCard = fromPile[move.FromRow];
            Card exposedCard = fromPile[move.FromRow - 1];
            if (!exposedCard.IsTargetFor(fromCard))
            {
                // Check whether the exposed card will be useful.
                int emptyPiles = EmptyPiles.Count - 1;
                int maxExtraSuits = ExtraSuits(emptyPiles);
                int fromSuits = fromPile.CountSuits(move.FromRow);
                for (int nextFrom = 0; nextFrom < NumberOfPiles; nextFrom++)
                {
                    if (nextFrom == move.From || nextFrom == move.To)
                    {
                        // Inappropriate column.
                        continue;
                    }
                    Pile nextFromPile = UpPiles[nextFrom];
                    if (nextFromPile.Count == 0)
                    {
                        // Column is empty.
                        continue;
                    }
                    int nextFromRow = nextFromPile.Count - RunLengthsAnySuit[nextFrom];
                    if (!nextFromPile[nextFromRow].IsSourceFor(exposedCard))
                    {
                        // Not the card we need.
                        continue;
                    }
                    int extraSuits = nextFromPile.CountSuits(nextFromRow) - 1;
                    if (extraSuits <= maxExtraSuits)
                    {
                        // Card leads to a useful move.
                        uses++;
                    }

                    // Check whether the exposed run will be useful.
                    int upperFromRow = move.FromRow - fromPile.GetRunUp(move.FromRow);
                    if (upperFromRow != move.FromRow)
                    {
                        Card upperFromCard = fromPile[upperFromRow];
                        uses += FaceLists[(int)upperFromCard.Face + 1].Count;
                    }
                }
            }
            return uses;
        }

        private int GetNetRunLength(int order, int from, int fromRow, int to, int toRow)
        {
            int moveRun = UpPiles.GetRunDown(from, fromRow);
            int fromRun = UpPiles.GetRunUp(from, fromRow + 1) + moveRun - 1;
            if (order != 2)
            {
                // The from card's suit doesn't match the to card's suit.
                if (moveRun == fromRun)
                {
                    // The from card's suit doesn't match its parent.
                    return 0;
                }
                return -fromRun;
            }
            int toRun = UpPiles.GetRunUp(to, toRow);
            int newRun = moveRun + toRun;
            if (moveRun == fromRun)
            {
                // The from card's suit doesn't match its parent.
                return newRun;
            }
            return newRun - fromRun;
        }

        private bool ChooseMove()
        {
            // We may be strictly out of moves.
            if (Candidates.Count == 0)
            {
                return false;
            }

            // Calculate scores.
            for (int i = 0; i < Candidates.Count; i++)
            {
                Move candidate = Candidates[i];
                candidate.Score = CalculateScore(candidate);
                Candidates[i] = candidate;
            }

            if (Diagnostics)
            {
                PrintGame();
                PrintViableCandidates();
                Utils.WriteLine("Moves.Count = {0}", Moves.Count);
            }

            Move move = Candidates[0];
            for (int i = 0; i < Candidates.Count; i++)
            {
                if (Candidates[i].Score > move.Score)
                {
                    move = Candidates[i];
                }
            }

            // The best move may not be worth making.
            if (move.Score == RejectScore)
            {
                return false;
            }

#if false
            if ((move.Flags & MoveFlags.Flagged) != 0)
            {
                Debugger.Break();
            }
#endif

            MoveProcessor.ProcessMove(move);

            return true;
        }

        public Move Normalize(Move move)
        {
            if (move.FromRow < 0)
            {
                move.FromRow += UpPiles[move.From].Count;
            }
            if (move.ToRow == -1)
            {
                move.ToRow = UpPiles[move.To].Count;
            }
            return move;
        }

        public void Discard()
        {
            for (int i = 0; i < NumberOfPiles; i++)
            {
                Pile pile = UpPiles[i];
                if (pile.Count < 13)
                {
                    continue;
                }
                if (pile[pile.Count - 1].Face != Face.Ace)
                {
                    continue;
                }

                int runLength = pile.GetRunUp(pile.Count);
                if (runLength == 13)
                {
                    if (Diagnostics)
                    {
                        Utils.WriteLine("*** discarding ***");
                    }
                    int row = pile.Count - runLength;
                    Pile discard = new Pile();
                    for (int j = 0; j < 13; j++)
                    {
                        discard.Add(pile[row + j]);
                    }
                    pile.RemoveRange(row, 13);
                    DiscardPiles.Add(discard);
                }
            }
        }

        public void TurnOverCards()
        {
            for (int i = 0; i < NumberOfPiles; i++)
            {
                Pile up = UpPiles[i];
                Pile down = DownPiles[i];
                if (up.Count == 0 && down.Count > 0)
                {
                    if (Diagnostics)
                    {
                        Utils.WriteLine("*** turning over card ***");
                    }
                    up.Add(down.Next());
                }
            }
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
            PrintGamesSideBySide(LastGame, this);
        }

        public void PrintMoves()
        {
            PrintMoves(Moves);
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
                if (move.Score != RejectScore)
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
            for (int holdingNext = move.HoldingNext; holdingNext != -1; holdingNext = HoldingList[holdingNext].Next)
            {
                Utils.WriteLine("    holding {0}", HoldingList[holdingNext]);
            }
        }

        public string ToAsciiString()
        {
            return GameInputOutput.ToAsciiString();
        }

        public void FromAsciiString(string s)
        {
            GameInputOutput.FromAsciiString(s);
        }

        public void FromGame(Game other)
        {
            GameInputOutput.FromGame(other);
        }

        public string ToPrettyString()
        {
            return GameInputOutput.ToPrettyString();
        }

        public static void PrintGamesSideBySide(Game game1, Game game2)
        {
            GameInputOutput.PrintGamesSideBySide(game1, game2);
        }

        public override string ToString()
        {
            return GameInputOutput.ToPrettyString();
        }
    }
}
