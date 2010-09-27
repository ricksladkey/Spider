using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    public class Game
    {
        public static double[] FourSuitCoefficients = new double[] {
            /* 0 */ 8.42581707, 42.35984891, -0.1201269187, -4.841970863, -0.1252077493, 4.908558385, 8.502830004, 1,
            /* 8 */ 2.241481832, 0.003208907113, -0.1594844085, -0.9196463991, 5.422359166, 1,
        };

        public static double[] TwoSuitCoefficients = new double[] {
            /* 0 */ 6.362452378, 63.49174024, -0.1027791635, -4.898312748, -0.741136128, 1.436684988, 9.447113329, 0.8164965809,
            /* 8 */ 1.71772823, 0.0002049519119, -0.05704045645, -0.2307120267, 1.49037565, 0.5019319713,
        };

        public static double[] OneSuitCoefficients = new double[] {
            /* 0 */ 4.241634919, 93.31341988, -0.08091391227, -3.265541832, -0.5942021654, 2.565712243, 17.64117551, 1,
            /* 8 */ 1.756489081, 0.0002561898898, -0.04347481483, -0.1737026135, 3.471266012, 1,
        };

        public const int NumberOfPiles = 10;
        public const int MaximumMoves = 1500;

        public const int Group0 = 0;
        public const int Group1 = 8;

        public static char Fence = '@';
        public static char PrimarySeparator = '|';
        public static char SecondarySeparator = '-';

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

        public Pile ScratchPile { get; private set; }
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

        private CompositeSinglePileMoveFinder CompositeSinglePileMoveFinder { get; set; }

        public List<ComplexMove> ComplexCandidates
        {
            get
            {
                List<ComplexMove> result = new List<ComplexMove>();
                for (int index = 0; index < Candidates.Count; index++)
                {
                    result.Add(new ComplexMove(index, Candidates, SupplementaryList, HoldingList));
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

            ScratchPile = new Pile();
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

            CompositeSinglePileMoveFinder = new CompositeSinglePileMoveFinder(this);
        }

        public Game(string game)
            : this()
        {
            FromAsciiString(game);
        }

        public Game(Game game)
            : this()
        {
            FromGame(game);
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

            int pile = 0;
            for (int i = 0; i < 44; i++)
            {
                DownPiles[pile].Add(StockPile.Next());
                pile = (pile + 1) % NumberOfPiles;
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
                HoldingStack.StartingIndex = fromPile.Count;
                int extraSuits = 0;
                for (int fromIndex = fromPile.Count - 1; fromIndex >= 0; fromIndex--)
                {
                    Card fromCard = fromPile[fromIndex];
                    if (fromIndex < fromPile.Count - 1)
                    {
                        Card previousCard = fromPile[fromIndex + 1];
                        if (previousCard.Face + 1 != fromCard.Face)
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
                                Candidates.Add(new Move(from, fromIndex, to, toPile.Count, AddHolding(holdingSet)));

                                // Update the holding pile move.
                                int holdingSuits = extraSuits;
                                if (fromIndex > 0 && (fromPile[fromIndex - 1].Face - 1 != fromCard.Face || fromCard.Suit != fromPile[fromIndex - 1].Suit))
                                {
                                    holdingSuits++;
                                }
                                if (holdingSuits > HoldingStack.Suits)
                                {
                                    int length = HoldingStack.Index - fromIndex;
                                    HoldingStack.Push(new HoldingInfo(from, fromIndex, to, holdingSuits, length));
                                }

                                break;
                            }
                        }
                    }

                    // Add moves to an empty pile.
                    for (int i = 0; i < EmptyPiles.Count; i++)
                    {
                        int to = EmptyPiles[0];

                        if (fromIndex == 0)
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
                            Card nextCard = fromPile[fromIndex - 1];
                            if (fromCard.Suit == nextCard.Suit)
                            {
                                if (nextCard.Face - 1 == fromCard.Face)
                                {
                                    continue;
                                }
                            }
                        }

                        foreach (HoldingSet holdingSet in HoldingStack.Sets)
                        {
                            if (holdingSet.Index == fromIndex)
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
                            Candidates.Add(new Move(from, fromIndex, to, toPile.Count, AddHolding(holdingSet)));
                            break;
                        }

                        // Only need to check the first empty pile
                        // since all empty piles are the same
                        // except for undealt cards.
                        break;
                    }

                    // Check for swaps.
                    CheckSwaps(from, fromIndex, extraSuits, maxExtraSuits);
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
                int fromIndex = fromPile.Count - RunLengthsAnySuit[from];
                if (fromIndex == 0)
                {
                    continue;
                }
                int fromSuits = fromPile.CountSuits(fromIndex);
                if (fromSuits - 1 > maxExtraSuits)
                {
                    continue;
                }
                Card fromCard = fromPile[fromIndex];
                PileList faceList = FaceLists[(int)fromCard.Face + 1];
                for (int i = 0; i < faceList.Count; i++)
                {
                    int to = faceList[i];
                    Pile toPile = UpPiles[to];
                    Card toCard = toPile[toPile.Count - 1];
                    int order = GetOrder(toCard, fromCard);
                    UncoveringMoves.Add(new Move(from, fromIndex, to, order));
                }
            }
        }

        private void FindOneRunPiles()
        {
            OneRunPiles.Clear();
            for (int i = 0; i < NumberOfPiles; i++)
            {
                int count = UpPiles[i].Count;
                if (count != 0 && count != RunLengthsAnySuit[i])
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

        private void CheckSwaps(int from, int fromIndex, int extraSuits, int maxExtraSuits)
        {
#if false
            if (extraSuits + 1 > maxExtraSuits + HoldingStack.Suits)
            {
                // Need at least one empty pile or a holding pile to swap.
                return;
            }
#endif
            if (fromIndex == 0 && DownPiles[from].Count != 0)
            {
                // Would turn over a card.
                return;
            }
            Pile fromPile = UpPiles[from];
            Card fromCard = fromPile[fromIndex];
            Card fromCardParent = Card.Empty;
            bool inSequence = true;
            if (fromIndex != 0)
            {
                fromCardParent = fromPile[fromIndex - 1];
                inSequence = fromCardParent.Face - 1 == fromCard.Face;
            }
            for (int to = 0; to < NumberOfPiles; to++)
            {
                Pile toPile = UpPiles[to];
                if (to == from || toPile.Count == 0)
                {
                    continue;
                }
                int splitIndex = toPile.Count - RunLengthsAnySuit[to];
                int toIndex = -1;
                if (inSequence)
                {
                    // Try to find from counterpart in the first to run.
                    toIndex = splitIndex + (int)(toPile[splitIndex].Face - fromCard.Face);
                    if (toIndex < splitIndex || toIndex >= toPile.Count)
                    {
                        // Sequence doesn't contain our counterpart.
                        continue;
                    }
                }
                else
                {
                    // Try to swap with both runs out of sequence.
                    toIndex = splitIndex;
                    if (fromIndex != 0 && fromCardParent.Face - 1 != toPile[toIndex].Face)
                    {
                        // Cards don't match.
                        continue;
                    }
                }
                if (toIndex == 0)
                {
                    if (fromIndex == 0)
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
                else if (toPile[toIndex - 1].Face - 1 != fromCard.Face)
                {
                    // Cards don't match.
                    continue;
                }

#if false
                int toSuits = toPile.CountSuits(toIndex);
                HoldingStack forwardHoldingStack = new HoldingStack();
                if (extraSuits + toSuits > maxExtraSuits)
                {
                    // Check whether forward holding piles will help.
                    int forwardHoldingSuits = FindHolding(UpPiles, forwardHoldingStack, true, from, fromIndex, fromPile.Count, to, maxExtraSuits);
                    if (extraSuits + toSuits > maxExtraSuits + forwardHoldingSuits)
                    {
                        // Prepare an accurate map.
                        CardMap map = new CardMap();
                        map.Update(UpPiles);
                        foreach (HoldingInfo holding in forwardHoldingStack.Set)
                        {
                            map[holding.To] = fromPile[holding.FromIndex + holding.Length - 1];
                        }

                        // Check whether reverse holding piles will help.
                        HoldingStack reverseHoldingStack = new HoldingStack();
                        int reverseHoldingSuits = FindHolding(map, reverseHoldingStack, true, to, toIndex, toPile.Count, from, maxExtraSuits);
                        if (extraSuits + toSuits > maxExtraSuits + forwardHoldingSuits + reverseHoldingSuits)
                        {
                            continue;
                        }

                        Candidates.Add(new Move(MoveType.Swap, from, fromIndex, to, toIndex, AddHolding(forwardHoldingStack.Set, reverseHoldingStack.Set)));
                        continue;
                    }
                }

                // We've found a legal swap.
                Debug.Assert(toIndex == 0 || toPile[toIndex - 1].Face - 1 == fromCard.Face);
                Debug.Assert(fromIndex == 0 || fromCardParent.Face - 1 == toPile[toIndex].Face);
                Candidates.Add(new Move(MoveType.Swap, from, fromIndex, to, toIndex, AddHolding(forwardHoldingStack.Set)));
#else
                int toSuits = toPile.CountSuits(toIndex);
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
                    Debug.Assert(toIndex == 0 || toPile[toIndex - 1].Face - 1 == fromCard.Face);
                    Debug.Assert(fromIndex == 0 || fromCardParent.Face - 1 == toPile[toIndex].Face);
                    Candidates.Add(new Move(MoveType.Swap, from, fromIndex, to, toIndex, AddHolding(holdingSet)));
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
                            map[holding.To] = fromPile[holding.FromIndex + holding.Length - 1];
                        }

                        HoldingStack reverseHoldingStack = new HoldingStack();
                        int reverseHoldingSuits = FindHolding(map, reverseHoldingStack, true, to, toIndex, toPile.Count, from, maxExtraSuits);
                        if (extraSuits + toSuits <= maxExtraSuits + holdingSet.Suits + reverseHoldingSuits)
                        {
                            Candidates.Add(new Move(MoveType.Swap, from, fromIndex, to, toIndex, AddHolding(holdingSet, reverseHoldingStack.Set)));
                        }
                    }
                }
#endif
            }
        }

        public int FindHolding(IGetCard map, HoldingStack holdingStack, bool inclusive, int from, int fromStart, int fromEnd, int to, int maxExtraSuits)
        {
            holdingStack.StartingIndex = fromEnd;
            Pile fromPile = UpPiles[from];
            int firstIndex = fromStart + (inclusive ? 0 : 1);
            int lastIndex = fromEnd - fromPile.GetRunUp(fromEnd);
            int extraSuits = 0;
            for (int fromIndex = lastIndex; fromIndex >= firstIndex; fromIndex--)
            {
                if (fromIndex < lastIndex &&
                    fromPile[fromIndex].Suit != fromPile[fromIndex + 1].Suit)
                {
                    extraSuits++;
                }
                if (extraSuits > maxExtraSuits)
                {
                    return holdingStack.Suits;
                }
                Card fromCard = fromPile[fromIndex];
                for (int pile = 0; pile < NumberOfPiles; pile++)
                {
                    if (pile == from || pile == to)
                    {
                        continue;
                    }
                    if (fromCard.Face + 1 == map.GetCard(pile).Face)
                    {
                        int holdingSuits = extraSuits;
                        if (fromIndex == fromStart || fromCard.Suit != fromPile[fromIndex - 1].Suit)
                        {
                            holdingSuits++;
                        }
                        if (holdingSuits > holdingStack.Suits)
                        {
                            int length = holdingStack.Index - fromIndex;
                            holdingStack.Push(new HoldingInfo(from, fromIndex, pile, holdingSuits, length));
                        }
                    }
                }
            }
            return holdingStack.Suits;
        }

        public static int ExtraSuits(int emptyPiles)
        {
#if true
            // The formula for how many intermediate runs can
            // be moved is m: = sum(1 + 2 + ... + n).
            return emptyPiles * (emptyPiles + 1) / 2;
#else
            // The formula for how many intermediate runs can
            // be moved is m: = sum(1 + 2 + ... + 2^(n - 1)).
            if (emptyPiles < 0)
            {
                return 0;
            }
            int power = 1;
            for (int i = 0; i < emptyPiles; i++)
            {
                power *= 2;
            }
            return power - 1;
#endif
        }

        public static int EmptyPilesUsed(int emptyPiles, int suits)
        {
            int used = 0;
            for (int n = emptyPiles; n > 0 && suits > 0; n--)
            {
                used++;
                suits -= n;
            }
            return used;
        }

        private int RoundUpExtraSuits(int suits)
        {
            int emptyPiles = 0;
            while (true)
            {
                int extraSuits = ExtraSuits(emptyPiles);
                if (extraSuits >= suits)
                {
                    return extraSuits;
                }
                emptyPiles++;
            }
        }

        private int FindEmptyPiles()
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
            int fromIndex = move.FromIndex;
            int to = move.To;
            int toIndex = move.ToIndex;

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
            Card fromParent = fromIndex != 0 ? fromPile[fromIndex - 1] : Card.Empty;
            Card fromChild = fromPile[fromIndex];
            Card toParent = toIndex != 0 ? toPile[toIndex - 1] : Card.Empty;
            Card toChild = toIndex != toPile.Count ? toPile[toIndex] : Card.Empty;
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
            bool wholePile = fromIndex == 0 && toIndex == toPile.Count;
            int netRunLengthFrom = GetNetRunLength(newOrderFrom, from, fromIndex, to, toIndex);
            int netRunLengthTo = isSwap ? GetNetRunLength(newOrderTo, to, toIndex, from, fromIndex) : 0;
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
                    delta = UpPiles.GetRunDelta(from, fromIndex, to, toIndex);
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

            score.Order = move.ToIndex;
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
            Card fromCard = fromPile[move.FromIndex];
            bool wholePile = move.FromIndex == 0;
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
            else if (fromPile[move.FromIndex - 1].Face - 1 == fromCard.Face)
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
            bool fromUpper = UpPiles.GetRunUp(move.From, move.FromIndex) == move.FromIndex;
            bool fromLower = move.HoldingNext == -1;
            bool toUpper = UpPiles.GetRunUp(move.To, move.ToIndex) == move.ToIndex;
            bool oldFrom = move.FromIndex == 0 ?
                (fromFree && fromLower) :
                (fromFree && fromUpper && fromLower && oldOrder == 2);
            bool newFrom = fromFree && fromUpper;
            bool oldTo = toFree && toUpper;
            bool newTo = move.ToIndex == 0 ?
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
            if (move.FromIndex == 0 || move.ToIndex != UpPiles[move.To].Count)
            {
                // No exposed card, no uses.
                return 0;
            }

            int uses = 0;

            Pile fromPile = UpPiles[move.From];
            Card fromCard = fromPile[move.FromIndex];
            Card exposedCard = fromPile[move.FromIndex - 1];
            if (exposedCard.Face - 1 != fromCard.Face)
            {
                // Check whether the exposed card will be useful.
                int emptyPiles = EmptyPiles.Count - 1;
                int maxExtraSuits = ExtraSuits(emptyPiles);
                int fromSuits = fromPile.CountSuits(move.FromIndex);
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
                    int nextFromIndex = nextFromPile.Count - RunLengthsAnySuit[nextFrom];
                    if (nextFromPile[nextFromIndex].Face + 1 != exposedCard.Face)
                    {
                        // Not the card we need.
                        continue;
                    }
                    int extraSuits = nextFromPile.CountSuits(nextFromIndex) - 1;
                    if (extraSuits <= maxExtraSuits)
                    {
                        // Card leads to a useful move.
                        uses++;
                    }

                    // Check whether the exposed run will be useful.
                    int upperFromIndex = move.FromIndex - fromPile.GetRunUp(move.FromIndex);
                    if (upperFromIndex != move.FromIndex)
                    {
                        Card upperFromCard = fromPile[upperFromIndex];
                        uses += FaceLists[(int)upperFromCard.Face + 1].Count;
                    }
                }
            }
            return uses;
        }

        public static int GetOrder(Card parent, Card child)
        {
            if (parent.Face - 1 != child.Face)
            {
                return 0;
            }
            if (parent.Suit != child.Suit)
            {
                return 1;
            }
            return 2;
        }

        public static int GetOrder(bool facesMatch, bool suitsMatch)
        {
            if (!facesMatch)
            {
                return 0;
            }
            if (!suitsMatch)
            {
                return 1;
            }
            return 2;
        }

        private int GetNetRunLength(int order, int from, int fromIndex, int to, int toIndex)
        {
            int moveRun = UpPiles.GetRunDown(from, fromIndex);
            int fromRun = UpPiles.GetRunUp(from, fromIndex + 1) + moveRun - 1;
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
            int toRun = UpPiles.GetRunUp(to, toIndex);
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

            if (RecordComplex)
            {
                AddMove(move);
            }

            if (ComplexMoves)
            {
                MakeMove(move);
            }
            else
            {
                ConvertToSimpleMoves(move);
            }

            return true;
        }

        private void ConvertToSimpleMoves(Move move)
        {
            if (Diagnostics)
            {
                Utils.WriteLine("CTSM: {0}", move);
            }

            // First move to the holding piles.
            Stack<Move> moveStack = new Stack<Move>();
            for (int holdingNext = move.HoldingNext; holdingNext != -1; holdingNext = HoldingList[holdingNext].Next)
            {
                HoldingInfo holding = HoldingList[holdingNext];
                int undoFromIndex = UpPiles[holding.To].Count;
                MakeMoveUsingEmptyPiles(holding.From, holding.FromIndex, holding.To);
                moveStack.Push(new Move(holding.To, undoFromIndex, holding.From == move.From ? move.To : move.From));
            }
            if (move.Type == MoveType.CompositeSinglePile)
            {
                // Composite single pile move.
                MakeCompositeSinglePileMove(move.Next);
            }
            else if (move.Type == MoveType.Swap)
            {
                // Swap move.
                SwapUsingEmptyPiles(move.From, move.FromIndex, move.To, move.ToIndex);
            }
            else
            {
                // Ordinary move.
                MakeMoveUsingEmptyPiles(move.From, move.FromIndex, move.To);
            }

            // Lastly move from the holding piles, if we still can.
            while (moveStack.Count > 0)
            {
                TryToMakeMoveUsingEmptyPiles(moveStack.Pop());
            }
        }

        private void MakeMove(Move move)
        {
            if (move.Next != -1)
            {
                for (int next = move.Next; next != -1; next = SupplementaryList[next].Next)
                {
                    Move subMove = SupplementaryList[next];
                    MakeSingleMove(subMove);
                }
                return;
            }
            MakeSingleMove(move);
        }

        private void SwapUsingEmptyPiles(int from, int fromIndex, int to, int toIndex)
        {
            if (Diagnostics)
            {
                Utils.WriteLine("SWUEP: {0}/{1} -> {2}/{3}", from, fromIndex, to, toIndex);
            }
            int emptyPiles = FindEmptyPiles();
            int fromSuits = UpPiles.CountSuits(from, fromIndex);
            int toSuits = UpPiles.CountSuits(to, toIndex);
            if (fromSuits == 0 && toSuits == 0)
            {
                return;
            }
            if (fromSuits + toSuits - 1 > ExtraSuits(emptyPiles))
            {
                throw new InvalidMoveException("insufficient empty piles");
            }
            Stack<Move> moveStack = new Stack<Move>();
            for (int n = emptyPiles; n > 0 && fromSuits + toSuits > 1; n--)
            {
                if (fromSuits >= toSuits)
                {
                    int moveSuits = toSuits != 0 ? fromSuits : fromSuits - 1;
                    fromSuits -= MoveOffUsingEmptyPiles(from, fromIndex, to, moveSuits, n, moveStack);
                }
                else
                {
                    int moveSuits = fromSuits != 0 ? toSuits : toSuits - 1;
                    toSuits -= MoveOffUsingEmptyPiles(to, toIndex, from, moveSuits, n, moveStack);
                }
            }
            if (fromSuits + toSuits != 1 || fromSuits * toSuits != 0)
            {
                throw new Exception("bug: left over swap runs");
            }
            if (fromSuits == 1)
            {
                MakeSimpleMove(from, fromIndex, to);
            }
            else
            {
                MakeSimpleMove(to, toIndex, from);
            }
            while (moveStack.Count != 0)
            {
                Move move = moveStack.Pop();
                MakeSimpleMove(move.From, move.FromIndex, move.To);
            }
        }

        private void UnloadToEmptyPiles(int from, int lastFromIndex, int to, Stack<Move> moveStack)
        {
            if (Diagnostics)
            {
                Utils.WriteLine("ULTEP: {0}/{1} -> {2}", from, lastFromIndex, to);
            }
            int emptyPiles = FindEmptyPiles();
            int suits = UpPiles.CountSuits(from, lastFromIndex);
            if (suits > ExtraSuits(emptyPiles))
            {
                throw new InvalidMoveException("insufficient empty piles");
            }
            int totalSuits = UpPiles.CountSuits(from, lastFromIndex);
            int remainingSuits = totalSuits;
            int fromIndex = UpPiles[from].Count;
            for (int n = 0; n < emptyPiles; n++)
            {
                int m = Math.Min(emptyPiles, n + remainingSuits);
                for (int i = m - 1; i >= n; i--)
                {
                    int runLength = UpPiles.GetRunUp(from, fromIndex);
                    fromIndex -= runLength;
                    fromIndex = Math.Max(fromIndex, lastFromIndex);
                    MakeSimpleMove(from, -runLength, EmptyPiles[i]);
                    moveStack.Push(new Move(EmptyPiles[i], -runLength, to));
                    remainingSuits--;
                }
                for (int i = n + 1; i < m; i++)
                {
                    int runLength = UpPiles[EmptyPiles[i]].Count;
                    MakeSimpleMove(EmptyPiles[i], -runLength, EmptyPiles[n]);
                    moveStack.Push(new Move(EmptyPiles[n], -runLength, EmptyPiles[i]));
                }
                if (remainingSuits == 0)
                {
                    break;
                }
            }
        }

        private int MoveOffUsingEmptyPiles(int from, int lastFromIndex, int to, int remainingSuits, int n, Stack<Move> moveStack)
        {
            int suits = Math.Min(remainingSuits, n);
            if (Diagnostics)
            {
                Utils.WriteLine("MOUEP: {0} -> {1}: {2}", from, to, suits);
            }
            for (int i = n - suits; i < n; i++)
            {
                // Move as much as possible but not too much.
                Pile fromPile = UpPiles[from];
                int fromIndex = fromPile.Count - UpPiles.GetRunUp(from, fromPile.Count);
                if (fromIndex < lastFromIndex)
                {
                    fromIndex = lastFromIndex;
                }
                int runLength = fromPile.Count - fromIndex;
                MakeSimpleMove(from, -runLength, EmptyPiles[i]);
                moveStack.Push(new Move(EmptyPiles[i], -runLength, to));
            }
            for (int i = n - 2; i >= n - suits; i--)
            {
                int runLength = UpPiles[EmptyPiles[i]].Count;
                MakeSimpleMove(EmptyPiles[i], -runLength, EmptyPiles[n - 1]);
                moveStack.Push(new Move(EmptyPiles[n - 1], -runLength, EmptyPiles[i]));
            }
            return suits;
        }

        private Move Normalize(Move move)
        {
            if (move.FromIndex < 0)
            {
                move.FromIndex += UpPiles[move.From].Count;
            }
            if (move.ToIndex == -1)
            {
                move.ToIndex = UpPiles[move.To].Count;
            }
            return move;
        }

        private void MakeCompositeSinglePileMove(int first)
        {
            if (Diagnostics)
            {
                Utils.WriteLine("MCSPM");
            }
            int offloadPile = -1;
            Stack<Move> moveStack = new Stack<Move>();
            for (int next = first; next != -1; next = SupplementaryList[next].Next)
            {
                int emptyPiles = FindEmptyPiles();
                Move move = Normalize(SupplementaryList[next]);
                if (move.Type == MoveType.Unload)
                {
                    offloadPile = move.To;
                    UnloadToEmptyPiles(move.From, move.FromIndex, -1, moveStack);
                }
                else if (move.Type == MoveType.Reload)
                {
                    if (Diagnostics)
                    {
                        Utils.WriteLine("RL:");
                    }
                    while (moveStack.Count != 0)
                    {
                        Move subMove = moveStack.Pop();
                        int to = subMove.To != -1 ? subMove.To : move.To;
                        MakeSimpleMove(subMove.From, subMove.FromIndex, to);
                    }
                    offloadPile = -1;

                }
                else if ((move.Flags & MoveFlags.UndoHolding) == MoveFlags.UndoHolding)
                {
                    TryToMakeMoveUsingEmptyPiles(move);
                }
                else
                {
                    if (!TryToMakeMoveUsingEmptyPiles(move))
                    {
                        // Things got messed up due to a discard.  There should
                        // be another pile with the same target.
                        bool foundAlternative = false;
                        Pile fromPile = UpPiles[move.From];
                        Card fromCard = fromPile[move.FromIndex];
                        for (int to = 0; to < NumberOfPiles; to++)
                        {
                            if (to == move.From)
                            {
                                continue;
                            }
                            Pile toPile = UpPiles[to];
                            if (toPile.Count == 0)
                            {
                                continue;
                            }
                            if (fromCard.Face + 1 != toPile[toPile.Count - 1].Face)
                            {
                                continue;
                            }
                            MakeMoveUsingEmptyPiles(move.From, move.FromIndex, to);
                            foundAlternative = true;
                            break;
                        }
                        if (!foundAlternative)
                        {
                            // This move is hopelessly messed up.
                            break;
                        }
                    }
                }
            }
            if (moveStack.Count != 0)
            {
                throw new Exception("missing reload move");
            }
        }

        private bool TryToMakeMoveUsingEmptyPiles(Move move)
        {
            if (SimpleMoveIsValid(move))
            {
                if (SafeMakeMoveUsingEmptyPiles(move.From, move.FromIndex, move.To) == null)
                {
                    return true;
                }
            }
            return false;
        }

        private bool SimpleMoveIsValid(Move move)
        {
            move = Normalize(move);
            int from = move.From;
            Pile fromPile = UpPiles[from];
            int fromIndex = move.FromIndex;
            int to = move.To;
            Pile toPile = UpPiles[to];
            int toIndex = toPile.Count;
            if (fromIndex < 0 || fromIndex >= fromPile.Count)
            {
                return false;
            }
            if (move.ToIndex != toPile.Count)
            {
                return false;
            }
            if (toPile.Count == 0)
            {
                return true;
            }
            if (fromPile[fromIndex].Face + 1 != toPile[toIndex - 1].Face)
            {
                return false;
            }
            return true;
        }

        private void MakeMovesUsingEmptyPiles(int first)
        {
            for (int next = first; next != -1; next = SupplementaryList[next].Next)
            {
                Move move = SupplementaryList[next];
                MakeMoveUsingEmptyPiles(move.From, move.FromIndex, move.To);
            }
        }

        private void MakeMoveUsingEmptyPiles(int from, int lastFromIndex, int to)
        {
            string error = SafeMakeMoveUsingEmptyPiles(from, lastFromIndex, to);
            if (error != null)
            {
                throw new InvalidMoveException(error);
            }
        }

        private string SafeMakeMoveUsingEmptyPiles(int from, int lastFromIndex, int to)
        {
            if (lastFromIndex < 0)
            {
                lastFromIndex += UpPiles[from].Count;
            }
            if (Diagnostics)
            {
                Utils.WriteLine("MMUEP: {0}/{1} -> {2}", from, lastFromIndex, to);
            }
            int toIndex = UpPiles[to].Count;
            int extraSuits = UpPiles.CountSuits(from, lastFromIndex) - 1;
            if (extraSuits < 0)
            {
                return "not a single run";
            }
            if (extraSuits == 0)
            {
                MakeSimpleMove(from, lastFromIndex, to);
                return null;
            }
            int emptyPiles = FindEmptyPiles();
            PileList usableEmptyPiles = new PileList(EmptyPiles);
            if (toIndex == 0)
            {
                usableEmptyPiles.Remove(to);
                emptyPiles--;
            }
            int maxExtraSuits = ExtraSuits(emptyPiles);
            if (extraSuits > maxExtraSuits)
            {
                return "insufficient empty piles";
            }
            int suits = 0;
            int fromIndex = UpPiles[from].Count;
            Stack<Move> moveStack = new Stack<Move>();
            for (int n = emptyPiles; n > 0; n--)
            {
                for (int i = 0; i < n; i++)
                {
                    int runLength = UpPiles.GetRunUp(from, fromIndex);
                    fromIndex -= runLength;
                    MakeSimpleMove(from, -runLength, usableEmptyPiles[i]);
                    moveStack.Push(new Move(usableEmptyPiles[i], -runLength, to));
                    suits++;
                    if (suits == extraSuits)
                    {
                        break;
                    }
                }
                if (suits == extraSuits)
                {
                    break;
                }
                for (int i = n - 2; i >= 0; i--)
                {
                    int runLength = UpPiles[usableEmptyPiles[i]].Count;
                    MakeSimpleMove(usableEmptyPiles[i], -runLength, usableEmptyPiles[n - 1]);
                    moveStack.Push(new Move(usableEmptyPiles[n - 1], -runLength, usableEmptyPiles[i]));
                }
            }
            MakeSimpleMove(from, lastFromIndex, to);
            while (moveStack.Count != 0)
            {
                Move move = moveStack.Pop();
                MakeSimpleMove(move.From, move.FromIndex, move.To);
            }
            return null;
        }

        private void MakeSimpleMove(int from, int fromIndex, int to)
        {
            if (fromIndex < 0)
            {
                fromIndex += UpPiles[from].Count;
            }
            if (Diagnostics)
            {
                Utils.WriteLine("    MSM: {0}/{1} -> {2}", from, fromIndex, to);
            }
            Debug.Assert(UpPiles[from].Count != 0);
            Debug.Assert(fromIndex < UpPiles[from].Count);
            Debug.Assert(UpPiles.CountSuits(from, fromIndex) == 1);
            Debug.Assert(UpPiles[to].Count == 0 || UpPiles[from][fromIndex].Face + 1 == UpPiles[to][UpPiles[to].Count - 1].Face);
            MakeMove(new Move(from, fromIndex, to, UpPiles[to].Count));
        }

        private void MakeSingleMove(Move move)
        {
            // Record the move.
            if (!RecordComplex)
            {
                AddMove(move);
            }

            // Make the moves.
            Pile fromPile = UpPiles[move.From];
            Pile toPile = UpPiles[move.To];
            Pile scratchPile = ScratchPile;
            int fromIndex = move.FromIndex;
            int fromCount = fromPile.Count - fromIndex;
            scratchPile.Clear();
            if (move.Type == MoveType.Swap)
            {
                int toIndex = move.ToIndex;
                int toCount = toPile.Count - toIndex;
                scratchPile.AddRange(toPile, toIndex, toCount);
                toPile.RemoveRange(toIndex, toCount);
                toPile.AddRange(fromPile, fromIndex, fromCount);
                fromPile.RemoveRange(fromIndex, fromCount);
                fromPile.AddRange(scratchPile, 0, toCount);
            }
            else if (move.Type == MoveType.Basic)
            {
                toPile.AddRange(fromPile, fromIndex, fromCount);
                fromPile.RemoveRange(fromIndex, fromCount);
            }
            else
            {
                throw new Exception("unsupported move type");
            }
            move.HoldingNext = -1;
            Discard();
            TurnOverCards();
        }

        private void AddMove(Move move)
        {
            move.Score = 0;
            if (TraceMoves)
            {
                Utils.WriteLine("Move {0}: {1}", Moves.Count, move);
            }
            Moves.Add(move);
        }

        private void Discard()
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
                    int index = pile.Count - runLength;
                    Pile discard = new Pile();
                    for (int j = 0; j < 13; j++)
                    {
                        discard.Add(pile[index + j]);
                    }
                    pile.RemoveRange(index, 13);
                    DiscardPiles.Add(discard);
                }
            }
        }

        private void TurnOverCards()
        {
            for (int i = 0; i < NumberOfPiles; i++)
            {
                Pile up = UpPiles[i];
                Pile down = DownPiles[i];
                if (up.Count == 0 && down.Count > 0)
                {
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

        public static void PrintGamesSideBySide(Game game1, Game game2)
        {
            string[] v1 = game1.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string[] v2 = game2.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            int max = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                max = Math.Max(max, v1[i].Length);
            }
            string text = "";
            for (int i = 0; i < v1.Length || i < v2.Length; i++)
            {
                string s1 = i < v1.Length ? v1[i] : "";
                string s2 = i < v2.Length ? v2[i] : "";
                text += s1.PadRight(max + 1) + s2 + Environment.NewLine;
            }
            Utils.ColorizeToConsole(text);
        }

        private void PrintMoves()
        {
            PrintMoves(Moves);
        }

        private void PrintMoves(MoveList moves)
        {
            foreach (Move move in moves)
            {
                PrintMove(move);
            }
        }

        private void PrintCandidates()
        {
            foreach (Move move in Candidates)
            {
                PrintMove(move);
            }
        }

        private void PrintViableCandidates()
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
            Pile discardRow = new Pile();
            for (int i = 0; i < DiscardPiles.Count; i++)
            {
                Pile discardPile = DiscardPiles[i];
                discardRow.Add(discardPile[discardPile.Count - 1]);
            }

            string s = "";

            s += Fence;
            s += Suits.ToString() + PrimarySeparator;
            s += ToAsciiString(discardRow) + PrimarySeparator;
            s += ToAsciiString(DownPiles) + PrimarySeparator;
            s += ToAsciiString(UpPiles) + PrimarySeparator;
            s += ToAsciiString(StockPile);
            s += Fence;

            return WrapString(s, 60);
        }

        private string WrapString(string s, int columns)
        {
            string t = "";
            while (s.Length > columns)
            {
                t += s.Substring(0, columns) + Environment.NewLine;
                s = s.Substring(columns);
            }
            return t + s;
        }

        private static string ToAsciiString(IList<Pile> rows)
        {
            string s = "";
            int n = rows.Count;
            while (n > 0 && rows[n - 1].Count == 0)
            {
                n--;
            }
            for (int i = 0; i < n; i++)
            {
                if (i != 0)
                {
                    s += SecondarySeparator;
                }
                s += ToAsciiString(rows[i]);
            }
            return s;
        }

        private static string ToAsciiString(Pile row)
        {
            string s = "";
            for (int i = 0; i < row.Count; i++)
            {
                s += row[i].ToAsciiString();
            }
            return s;
        }

        public void FromAsciiString(string s)
        {
            // Parse string.
            StringBuilder b = new StringBuilder();
            int i;
            for (i = 0; i < s.Length && s[i] != Fence; i++)
            {
            }
            if (i == s.Length)
            {
                throw new Exception("missing opening fence");
            }
            for (i++; i < s.Length && s[i] != Fence; i++)
            {
                char c = s[i];
                if (!char.IsWhiteSpace(c))
                {
                    b.Append(s[i]);
                }
            }
            if (i == s.Length)
            {
                throw new Exception("missing closing fence");
            }
            s = b.ToString();
            string[] sections = s.Split(PrimarySeparator);
            if (sections.Length != 5)
            {
                throw new Exception("wrong number of sections");
            }

            // Parse sections.
            int suits = int.Parse(sections[0]);
            if (suits != 1 && suits != 2 && suits != 4)
            {
                throw new Exception("invalid number of suits");
            }
            Pile discards = GetPileFromAsciiString(sections[1]);
            Pile[] downPiles = GetPilesFromAsciiString(sections[2]);
            Pile[] upPiles = GetPilesFromAsciiString(sections[3]);
            Pile stock = GetPileFromAsciiString(sections[4]);
            if (discards.Count > 8)
            {
                throw new Exception("too many discard piles");
            }
            if (downPiles.Length > NumberOfPiles)
            {
                throw new Exception("wrong number of down piles");
            }
            if (upPiles.Length > NumberOfPiles)
            {
                throw new Exception("wrong number of up piles");
            }
            if (stock.Count > 50)
            {
                throw new Exception("too many stock pile cards");
            }

            // Prepare game.
            Suits = suits;
            Initialize();
            foreach (Card discardCard in discards)
            {
                Pile discardPile = new Pile();
                for (Face face = Face.King; face >= Face.Ace; face--)
                {
                    discardPile.Add(new Card(face, discardCard.Suit));
                }
                DiscardPiles.Add(discardPile);
            }
            for (int pile = 0; pile < downPiles.Length; pile++)
            {
                DownPiles[pile] = downPiles[pile];
            }
            for (int pile = 0; pile < upPiles.Length; pile++)
            {
                UpPiles[pile] = upPiles[pile];
            }
            StockPile = stock;
        }

        private static Pile[] GetPilesFromAsciiString(string s)
        {
            string[] rows = s.Split(SecondarySeparator);
            int n = rows.Length;
            Pile[] piles = new Pile[n];
            for (int i = 0; i < n; i++)
            {
                piles[i] = GetPileFromAsciiString(rows[i]);
            }
            return piles;
        }

        private static Pile GetPileFromAsciiString(string s)
        {
            int n = s.Length / 2;
            Pile pile = new Pile();
            for (int i = 0; i < n; i++)
            {
                pile.Add(Utils.GetCard(s.Substring(2 * i, 2)));
            }
            return pile;
        }

        public void FromGame(Game game)
        {
            Suits = game.Suits;
            Initialize();
            foreach (Pile pile in game.DiscardPiles)
            {
                DiscardPiles.Add(pile);
            }
            for (int pile = 0; pile < NumberOfPiles; pile++)
            {
                DownPiles[pile].Copy((game.DownPiles[pile]));
            }
            for (int pile = 0; pile < NumberOfPiles; pile++)
            {
                UpPiles[pile].Copy((game.UpPiles[pile]));
            }
            StockPile.Copy((game.StockPile));
        }

        public string ToPrettyString()
        {
            string s = Environment.NewLine;
            s += "   Spider";
            s += Environment.NewLine;
            s += "--------------------------------";
            s += Environment.NewLine;
            Pile discardRow = new Pile();
            for (int i = 0; i < DiscardPiles.Count; i++)
            {
                Pile discardPile = DiscardPiles[i];
                discardRow.Add(discardPile[discardPile.Count - 1]);
            }
            s += ToPrettyString(-1, discardRow);
            s += Environment.NewLine;
            s += ToPrettyString(DownPiles);
            s += Environment.NewLine;
            s += "   0  1  2  3  4  5  6  7  8  9";
            s += Environment.NewLine;
            s += ToPrettyString(UpPiles);
            s += Environment.NewLine;
            for (int i = 0; i < StockPile.Count / NumberOfPiles; i++)
            {
                Pile row = new Pile();
                for (int j = 0; j < NumberOfPiles; j++)
                {
                    int index = i * NumberOfPiles + j;
                    int reverseIndex = StockPile.Count - index - 1;
                    row.Add(StockPile[reverseIndex]);
                }
                s += ToPrettyString(i, row);
            }

            return s;
        }

        private static string ToPrettyString(IList<Pile> rows)
        {
            string s = "";
            int max = 0;
            for (int i = 0; i < NumberOfPiles; i++)
            {
                max = Math.Max(max, rows[i].Count);
            }
            for (int j = 0; j < max; j++)
            {
                Pile row = new Pile();
                for (int i = 0; i < NumberOfPiles; i++)
                {
                    if (j < rows[i].Count)
                    {
                        row.Add(rows[i][j]);
                    }
                    else
                    {
                        row.Add(Card.Empty);
                    }
                }
                s += ToPrettyString(j, row);
            }
            return s;
        }

        private static string ToPrettyString(int index, Pile row)
        {
            string s = "";
            if (index == -1)
            {
                s += "   ";
            }
            else
            {
                s += string.Format("{0,2} ", index);
            }
            for (int i = 0; i < row.Count; i++)
            {
                if (i > 0)
                {
                    s += " ";
                }
                s += (row[i].IsEmpty) ? "  " : row[i].ToString();
            }
            return s + Environment.NewLine;
        }

        public override string ToString()
        {
            return ToPrettyString();
        }
    }
}
