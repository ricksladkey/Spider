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
            /* 0 */ 11.19118068, 33.21235313, -0.1201269187, -3.227980575, -0.1878116239, 7.070271551,
            /* 6 */ 2.430823772, 0.004813360669, -0.2034103221, -0.6384674211, 3.76449,
        };

        public static double[] TwoSuitCoefficients = new double[] {
            /* 0 */ 6.625729028, 76.19008829, -0.1263931638, -7.347469121, -0.7892200528, 4.526220521,
            /* 6 */ 2.634733621, 0.001296961317, -0.1150420899, -0.3908308805, 3.76449,
        };

        public static double[] OneSuitCoefficients = TwoSuitCoefficients;

        public const int NumberOfPiles = 10;
        public const int MaximumMoves = 1500;

        public const int Group0 = 0;
        public const int Group1 = 6;

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
        public Pile[] DownPiles { get; private set; }
        public Pile[] UpPiles { get; private set; }
        public List<Pile> DiscardPiles { get; private set; }

        private Pile ScratchPile { get; set; }
        private MoveList Candidates { get; set; }
        private MoveList SupplementaryMoves { get; set; }
        private MoveList SupplementaryList { get; set; }
        private HoldingStack HoldingStack { get; set; }
        private List<HoldingInfo> HoldingList { get; set; }
        private int[] RunLengths { get; set; }
        private int[] RunLengthsAnySuit { get; set; }
        private PileList FreeCells { get; set; }
        private PileList[] FaceLists { get; set; }
        private Game LastGame { get; set; }

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

        public int EmptyFreeCells
        {
            get
            {
                Analyze();
                return FreeCells.Count;
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
            DownPiles = new Pile[NumberOfPiles];
            UpPiles = new Pile[NumberOfPiles];
            for (int i = 0; i < NumberOfPiles; i++)
            {
                DownPiles[i] = new Pile();
                UpPiles[i] = new Pile();
            }
            DiscardPiles = new List<Pile>();

            ScratchPile = new Pile();
            Candidates = new MoveList();
            SupplementaryMoves = new MoveList();
            SupplementaryList = new MoveList();
            HoldingStack = new HoldingStack();
            HoldingList = new List<HoldingInfo>();
            RunLengths = new int[NumberOfPiles];
            RunLengthsAnySuit = new int[NumberOfPiles];
            FreeCells = new PileList();
            FaceLists = new PileList[(int)Face.King + 2];
            for (int i = 0; i < FaceLists.Length; i++)
            {
                FaceLists[i] = new PileList();
            }
            Coefficients = null;
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
                    if (!Move())
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
            for (int i = 0; i < NumberOfPiles; i++)
            {
                DownPiles[i].Clear();
                UpPiles[i].Clear();
            }
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

        public bool Move()
        {
            Candidates.Clear();
            SupplementaryList.Clear();
            HoldingList.Clear();

            Analyze();

            if (FreeCells.Count == NumberOfPiles)
            {
                Won = true;
                return true;
            }

            int freeCells = FreeCells.Count;
            int maxExtraSuits = ExtraSuits(freeCells);
            int maxExtraSuitsToFreeCell = ExtraSuits(freeCells - 1);

            for (int from = 0; from < NumberOfPiles; from++)
            {
                Pile fromPile = UpPiles[from];
                HoldingStack.Clear();
                HoldingStack.StartingIndex = fromPile.Count;
                int extraSuits = 0;
                int runLength = 0;
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
                            runLength = 0;
                            if (extraSuits > maxExtraSuits + HoldingStack.Suits)
                            {
                                break;
                            }
                        }
                    }
                    runLength++;

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
                                    HoldingStack.Push(new HoldingInfo(to, fromIndex, holdingSuits, length));
                                }

                                break;
                            }
                        }
                    }

                    // Add moves to a free cell.
                    for (int i = 0; i < FreeCells.Count; i++)
                    {
                        int to = FreeCells[0];

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
                            if (extraSuits > maxExtraSuitsToFreeCell + holdingSet.Suits)
                            {
                                // Not enough free cells.
                                continue;
                            }

                            // We've found a legal move.
                            Pile toPile = UpPiles[to];
                            Candidates.Add(new Move(from, fromIndex, to, toPile.Count, AddHolding(holdingSet)));
                            break;
                        }

                        // Only need to check the first free cell
                        // since all free cells are the same
                        // except for undealt cards.
                        break;
                    }

                    // Check for swaps.
                    CheckSwaps(from, fromIndex, extraSuits, maxExtraSuits);
                }

                // Check for composite single pile moves.
                CheckCompositeSinglePile(from);
            }

            return ChooseMove();
        }

        private void PrepareToDeal()
        {
        }

        private void RespondToDeal()
        {
        }

        private int AddSupplementary()
        {
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
            int first = holdingSet.Count == 0 ? -1 : HoldingList.Count;
            for (int i = 0; i < holdingSet.Count; i++)
            {
                HoldingInfo holdingInfo = holdingSet[i];
                if (i < holdingSet.Count - 1)
                {
                    holdingInfo.Next = HoldingList.Count + 1;
                }
                HoldingList.Add(holdingInfo);
            }
            return first;
        }

        private void CheckSwaps(int from, int fromIndex, int extraSuits, int maxExtraSuits)
        {
            if (extraSuits + 1 > maxExtraSuits + HoldingStack.Suits)
            {
                // Need at least one free cell or a holding pile to swap.
                return;
            }
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

                int toSuits = CountSuits(to, toIndex);
                foreach (HoldingSet holdingSet in HoldingStack.Sets)
                {
                    if (holdingSet.Contains(to))
                    {
                        // The pile is already in use.
                        continue;
                    }
                    if (extraSuits + toSuits > maxExtraSuits + holdingSet.Suits)
                    {
                        // Not enough free cells.
                        continue;
                    }

                    // We've found a legal swap.
                    Debug.Assert(toIndex == 0 || toPile[toIndex - 1].Face - 1 == fromCard.Face);
                    Debug.Assert(fromIndex == 0 || fromCardParent.Face - 1 == toPile[toIndex].Face);
                    Candidates.Add(new Move(MoveType.Swap, from, fromIndex, to, toIndex, AddHolding(holdingSet)));
                    break;
                }
            }
        }

        private void CheckCompositeSinglePile(int from)
        {
            int freeCells = FreeCells.Count;
            Pile fromPile = UpPiles[from];
            if (fromPile.Count == 0)
            {
                // No cards.
                return;
            }

            // Find roots.
            PileList roots = new PileList();
            int index = fromPile.Count;
            roots.Add(index);
            while (index > 0)
            {
                int count = GetRunUpAnySuit(from, index);
                index -= count;
                if (fromPile[index].Face == Face.King)
                {
                    // Cannot move a king.
                    return;
                }
                roots.Add(index);
            }
            int runs = roots.Count - 1;
            if (runs <= 1)
            {
                // Not at least two runs.
                return;
            }

            // Prepare data structures.
            int freeCellsLeft = freeCells;
            OffloadInfo offload = OffloadInfo.Empty;
            MoveList moves = SupplementaryMoves;
            moves.Clear();
            PileInfo[] map = new PileInfo[NumberOfPiles];

            // Initialize the pile map.
            for (int pile = 0; pile < NumberOfPiles; pile++)
            {
                if (pile != from)
                {
                    map[pile].Update(UpPiles[pile]);
                }
            }

            // Check all the roots.
            int offloads = 0;
            HoldingStack holdingStack = new HoldingStack();
            for (int n = 1; n < roots.Count; n++)
            {
                int rootIndex = roots[n];
                Card rootCard = fromPile[rootIndex];
                int runLength = roots[n - 1] - roots[n];
                int suits = CountSuits(from, rootIndex, rootIndex + runLength);
                int maxExtraSuits = ExtraSuits(freeCellsLeft);
                bool suitsMatch = false;
                holdingStack.Clear();

                // Try to find the best matching target.
                int to = -1;
                for (int i = 0; i < NumberOfPiles; i++)
                {
                    if (map[i].Last.Face - 1 == rootCard.Face)
                    {
                        if (map[i].Last.Suit == rootCard.Suit)
                        {
                            to = i;
                            suitsMatch = true;
                            break;
                        }
                        if (to == -1)
                        {
                            to = i;
                        }
                    }
                }

                MoveType type = MoveType.Basic;
                if (to != -1)
                {
                    // Check for inverting.
                    if (!offload.IsEmpty && to == offload.Pile)
                    {
                        if (!offload.CanInvert)
                        {
                            // Not enough free cells to invert.
                            return;
                        }

                        // Update the state.
                        offload.Suits += suits - (suitsMatch ? 1 : 0);
                    }

                    // Try to move this run.
                    if (suits - 1 > maxExtraSuits)
                    {
                        // Try using holding piles.
                        suits -= FindHolding(map, holdingStack, from, rootIndex, rootIndex + runLength, to, maxExtraSuits);
                        if (suits - 1 > maxExtraSuits)
                        {
                            // Not enough free cells.
                            return;
                        }
                    }
                }
                else
                {
                    if (!offload.IsEmpty)
                    {
                        // Already have an offload.
                        return;
                    }

                    // It doesn't make sense to offload the last root.
                    if (n == roots.Count - 1)
                    {
                        if (runs - 1 >= 2)
                        {
                            AddCompositeSinglePileMove(MoveFlags.Empty, from);
                        }
                        return;
                    }

                    // Check for partial offload.
                    if (offloads > 0)
                    {
                        AddCompositeSinglePileMove(MoveFlags.Empty, from);
                    }

                    // Try to offload this run.
                    if (freeCellsLeft == 0)
                    {
                        // Not enough free cells.
                        return;
                    }
                    to = FreeCells[0];
                    Debug.Assert(map[to].Count == 0);
                    if (suits > maxExtraSuits)
                    {
                        // Try using holding piles.
                        suits -= FindHolding(map, holdingStack, from, rootIndex, rootIndex + runLength, to, maxExtraSuits);
                        if (suits > maxExtraSuits)
                        {
                            // Still not enough free cells.
                            return;
                        }
                    }
                    int freeCellsUsed = FreeCellsUsed(freeCellsLeft, suits);
                    freeCellsLeft -= freeCellsUsed;
                    offload = new OffloadInfo(n, to, suits, freeCellsUsed);
                    type = offload.CanInvert ? MoveType.Basic : MoveType.Unload;
                    offloads++;
                }

                // Extract the holding set.
                HoldingSet holdingSet = holdingStack.Set;
                bool undoHolding = map[to].Count != 0;
                int remainingLength = runLength - holdingSet.Length;

                if (undoHolding)
                {
                    // Add moves to the holding piles.
                    foreach (HoldingInfo holding in holdingSet.Forwards)
                    {
                        moves.Add(new Move(MoveType.Basic, MoveFlags.Holding, from, -holding.Length, holding.Pile));
                    }

                    // Add the move.
                    moves.Add(new Move(type, from, rootIndex, to));

                    // Undo moves to the holding piles.
                    int toOffset = remainingLength;
                    foreach (HoldingInfo holding in holdingSet.Backwards)
                    {
                        moves.Add(new Move(MoveType.Basic, MoveFlags.UndoHolding, holding.Pile, -holding.Length, to));
                        toOffset += holding.Length;
                    }
                    Debug.Assert(toOffset == runLength);

                    // Update the map.
                    map[to].Last = fromPile[rootIndex + runLength - 1];
                    map[to].Count += runLength;
                }
                else
                {
                    // Add moves to the holding piles.
                    foreach (HoldingInfo holding in holdingSet.Forwards)
                    {
                        moves.Add(new Move(MoveType.Basic, MoveFlags.Holding, from, holding.Index, holding.Pile, map[holding.Pile].Count));

                        map[holding.Pile].Last = fromPile[holding.Index + holding.Length - 1];
                        map[holding.Pile].Count += holding.Length;
                    }

                    // Add the move.
                    moves.Add(new Move(type, from, rootIndex, to, map[to].Count));

                    // Update the map.
                    map[to].Last = fromPile[rootIndex + remainingLength - 1];
                    map[to].Count += remainingLength;
                }

                if (rootIndex == 0 && DownPiles[from].Count == 0)
                {
                    // Got to the bottom of the pile
                    // and created a free cell.
                    freeCellsLeft++;
                }

                if (offload.IsEmpty)
                {
                    // No offload to check.
                    continue;
                }

                int offloadRootIndex = roots[offload.Root];
                Card offloadRootCard = fromPile[offloadRootIndex];

                if (offload.CanInvert && offload.Suits - 1 > ExtraSuits(freeCellsLeft))
                {
                    // Can't move the offload due to inverting.
                    continue;
                }

                if (rootIndex > 0 && offloadRootCard.Face + 1 == fromPile[rootIndex - 1].Face)
                {
                    // Offload matches from pile.
                    moves.Add(new Move(offload.CanInvert ? MoveType.Basic : MoveType.Reload, offload.Pile, 0, from));
                    AddCompositeSinglePileMove(MoveFlags.Empty, from);
                    moves.RemoveAt(moves.Count - 1);
                }

                if (offloadRootCard.Face + 1 != map[to].Last.Face)
                {
                    // Cards don't match.
                    continue;
                }

                // Found a home for the offload.
                MoveType offloadType = offload.CanInvert ? MoveType.Basic : MoveType.Reload;
                moves.Add(new Move(offloadType, offload.Pile, 0, to));

                // Update the map.
                map[to].Last = map[offload.Pile].Last;
                map[to].Count += map[offload.Pile].Count;
                map[offload.Pile] = PileInfo.Empty;

                // Update the state.
                freeCellsLeft += offload.FreeCells;
                offload = OffloadInfo.Empty;
            }

            // Check for unload that needs to be reloaded.
            if (!offload.IsEmpty && !offload.CanInvert)
            {
                if (DownPiles[from].Count != 0)
                {
                    // Can't reload.
                    return;
                }
                else
                {
                    // Reload the offload onto the now empty pile.
                    moves.Add(new Move(MoveType.Reload, offload.Pile, 0, from, 0));
                }
            }

            // Determine move type.
            int downCount = DownPiles[from].Count;
            MoveFlags flags = MoveFlags.Empty;
            if (downCount != 0)
            {
                flags |= MoveFlags.TurnsOverCard;
            }
            if (offload.IsEmpty && downCount == 0)
            {
                flags |= MoveFlags.CreatesFreeCell;
            }
            if (!offload.IsEmpty && downCount != 0)
            {
                flags |= MoveFlags.UsesFreeCell;
            }
            AddCompositeSinglePileMove(flags, from);
        }

        private void AddCompositeSinglePileMove(MoveFlags flags, int from)
        {
            // Add the scoring move.
            Candidates.Add(new Move(MoveType.CompositeSinglePile, flags, from, 0, from, 0, -1, AddSupplementary()));
        }

        private int FindHolding(PileInfo[] map, HoldingStack holdingStack, int from, int fromStart, int fromEnd, int to, int maxExtraSuits)
        {
            holdingStack.StartingIndex = fromEnd;
            Pile fromPile = UpPiles[from];
            int firstIndex = fromStart + 1;
            int lastIndex = fromEnd - GetRunUp(from, fromEnd);
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
                    if (fromCard.Face + 1 == map[pile].Last.Face)
                    {
                        int holdingSuits = extraSuits;
                        if (fromCard.Suit != fromPile[fromIndex - 1].Suit)
                        {
                            holdingSuits++;
                        }
                        if (holdingSuits > holdingStack.Suits)
                        {
                            int length = holdingStack.Index - fromIndex;
                            holdingStack.Push(new HoldingInfo(pile, fromIndex, holdingSuits, length));
                        }
                    }
                }
            }
            return holdingStack.Suits;
        }

        private int ExtraSuits(int freeCells)
        {
#if true
            // The formula for how many intermediate runs can
            // be moved is m: = sum(1 + 2 + ... + n).
            return freeCells * (freeCells + 1) / 2;
#else
            // The formula for how many intermediate runs can
            // be moved is m: = sum(1 + 2 + ... + 2^(n - 1)).
            if (freeCells < 0)
            {
                return 0;
            }
            int power = 1;
            for (int i = 0; i < freeCells; i++)
            {
                power *= 2;
            }
            return power - 1;
#endif
        }

        private int FreeCellsUsed(int freeCells, int suits)
        {
            int used = 0;
            for (int n = freeCells; n > 0 && suits > 0; n--)
            {
                used++;
                suits -= n;
            }
            return used;
        }

        private int RoundUpExtraSuits(int suits)
        {
            int freeCells = 0;
            while (true)
            {
                int extraSuits = ExtraSuits(freeCells);
                if (extraSuits >= suits)
                {
                    return extraSuits;
                }
                freeCells++;
            }
        }

        private void Analyze()
        {
            FreeCells.Clear();
            for (int i = (int)Face.Ace; i <= (int)Face.King; i++)
            {
                FaceLists[i].Clear();
            }

            for (int i = 0; i < NumberOfPiles; i++)
            {
                // Prepare free cells and face lists.
                Pile pile = UpPiles[i];
                if (pile.Count == 0)
                {
                    FreeCells.Add(i);
                }
                else
                {
                    FaceLists[(int)pile[pile.Count - 1].Face].Add(i);
                }

                // Cache run lengths.
                RunLengths[i] = GetRunUp(i, pile.Count);
                RunLengthsAnySuit[i] = GetRunUpAnySuit(i, pile.Count);
            }
        }

        private double CalculateScore(Move move)
        {
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
            int order = newOrderFrom - oldOrderFrom + newOrderTo - oldOrderTo;
            if (order < 0)
            {
                return RejectScore;
            }
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
            score.CreatesFreeCell = wholePile && score.DownCount == 0;
            score.NoFreeCells = FreeCells.Count == 0;
            if (order == 0 && score.NetRunLength < 0)
            {
                return RejectScore;
            }
            int delta = 0;
            if (order == 0 && score.NetRunLength == 0)
            {
                if (!isSwap && oldOrderFrom == 1 && newOrderFrom == 1)
                {
                    delta = GetRunDelta(from, fromIndex, to, toIndex);
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
            Move firstMove = Normalize(SupplementaryList[move.Next]);

            score.FaceValue = (int)UpPiles[firstMove.From][firstMove.FromIndex].Face;
            score.NetRunLength = 0;
            score.DownCount = DownPiles[move.From].Count;
            score.TurnsOverCard = (move.Flags & MoveFlags.TurnsOverCard) == MoveFlags.TurnsOverCard;
            score.CreatesFreeCell = (move.Flags & MoveFlags.CreatesFreeCell) == MoveFlags.CreatesFreeCell;
            score.UsesFreeCell = (move.Flags & MoveFlags.UsesFreeCell) == MoveFlags.UsesFreeCell;
            score.IsCompositeSinglePile = true;
            score.NoFreeCells = FreeCells.Count == 0;
            score.OneRunDelta = 0;

            if (score.UsesFreeCell)
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
            score.UsesFreeCell = true;
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
            bool fromUpper = GetRunUp(move.From, move.FromIndex) == move.FromIndex;
            bool fromLower = move.HoldingNext == -1;
            bool toUpper = GetRunUp(move.To, move.ToIndex) == move.ToIndex;
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

        private int GetRunDelta(int from, int fromIndex, int to, int toIndex)
        {
            return GetRunUp(from, fromIndex) - GetRunUp(to, toIndex);
        }

        private int CountUses(Move move)
        {
            if (move.FromIndex == 0)
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
                int freeCells = FreeCells.Count - 1;
                int maxExtraSuits = ExtraSuits(freeCells);
                int fromSuits = CountSuits(move.From, move.FromIndex);
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
                    int extraSuits = CountSuits(nextFrom, nextFromIndex) - 1;
                    if (extraSuits <= maxExtraSuits)
                    {
                        // Card leads to a useful move.
                        uses++;
                    }

                    // Check whether the exposed run will be useful.
                    int upperFromIndex = move.FromIndex - GetRunUp(move.From, move.FromIndex);
                    if (upperFromIndex != move.FromIndex)
                    {
                        Card upperFromCard = fromPile[upperFromIndex];
                        uses += FaceLists[(int)upperFromCard.Face + 1].Count;
                    }
                }
            }
            return uses;
        }

        private int GetOrder(Card parent, Card child)
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

        private int GetNetRunLength(int order, int from, int fromIndex, int to, int toIndex)
        {
            int moveRun = GetRunDown(from, fromIndex);
            int fromRun = GetRunUp(from, fromIndex + 1) + moveRun - 1;
            if (order != 2)
            {
                // The from card's suit doesn't match the to card's suit.
                if (moveRun == fromRun)
                {
                    // The from card's suit doesn't its parent.
                    return 0;
                }
                return -fromRun;
            }
            int toRun = GetRunUp(to, toIndex);
            int newRun = moveRun + toRun;
            if (moveRun == fromRun)
            {
                // The from card's suit doesn't its parent.
                return newRun;
            }
            return newRun - fromRun;
        }

        private int CountSuits(int column, int row)
        {
            return CountSuits(column, row, -1);
        }

        private int CountSuits(int column, int startRow, int endRow)
        {
            Pile pile = UpPiles[column];
            if (endRow == -1)
            {
                endRow = UpPiles[column].Count;
            }
            Debug.Assert(startRow >= 0 && startRow <= pile.Count);
            Debug.Assert(endRow >= 0 && endRow <= pile.Count);
            int suits = 0;
            int index = startRow;
            if (index < endRow)
            {
                suits++;
                index += GetRunDown(column, index);
            }
            while (index < endRow)
            {
                if (pile[index - 1].Face - 1 != pile[index].Face)
                {
                    // Found an out of sequence run in the range.
                    return -1;
                }
                suits++;
                index += GetRunDown(column, index);
            }
            return suits;
        }

        private int GetRunUp(int column, int row)
        {
            if (row == 0)
            {
                return 0;
            }
            Pile pile = UpPiles[column];
            Debug.Assert(row >= 0 && row <= pile.Count);
            int runLength = 1;
            for (int index = row - 2; index >= 0; index--)
            {
                Card card = pile[index];
                Card nextCard = pile[index + 1];
                if (nextCard.Suit != card.Suit)
                {
                    break;
                }
                if (nextCard.Face + 1 != card.Face)
                {
                    break;
                }
                runLength++;
            }
            return runLength;
        }

        private int GetRunUpAnySuit(int column, int row)
        {
            if (row == 0)
            {
                return 0;
            }
            Pile pile = UpPiles[column];
            Debug.Assert(row >= 0 && row <= pile.Count);
            int runLength = 1;
            for (int index = row - 2; index >= 0; index--)
            {
                Card card = pile[index];
                Card nextCard = pile[index + 1];
                if (nextCard.Face + 1 != card.Face)
                {
                    break;
                }
                runLength++;
            }
            return runLength;
        }

        private int GetRunDown(int column, int row)
        {
            Pile pile = UpPiles[column];
            Debug.Assert(row >= 0 && row <= pile.Count);
            if (row == pile.Count)
            {
                return 0;
            }
            int runLength = 1;
            for (int index = row + 1; index < pile.Count; index++)
            {
                Card previousCard = pile[index - 1];
                Card card = pile[index];
                if (previousCard.Suit != card.Suit)
                {
                    break;
                }
                if (previousCard.Face - 1 != card.Face)
                {
                    break;
                }
                runLength++;
            }
            return runLength;
        }

        private int GetRunDownAnySuit(int column, int row)
        {
            Pile pile = UpPiles[column];
            if (row == pile.Count)
            {
                return 0;
            }
            int runLength = 1;
            for (int index = row + 1; index < pile.Count; index++)
            {
                Card previousCard = pile[index - 1];
                Card card = pile[index];
                if (previousCard.Face - 1 != card.Face)
                {
                    break;
                }
                runLength++;
            }
            return runLength;
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

#if false
            for (int i = 0; i < Candidates.Count; i++)
            {
                Move move1 = Candidates[i];
                if (move1.Type == MoveType.CompositeSinglePile)
                {
                    for (int j = i + 1; j < Candidates.Count; j++)
                    {
                        Move move2 = Candidates[j];
                        if (move2.Type == MoveType.CompositeSinglePile && move1.From == move2.From)
                        {
                            PrintViableCandidates();
                            Debugger.Break();
                            Console.WriteLine("two csp moves for one pile");
                        }
                    }
                }
            }
#endif

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
                int undoFromIndex = UpPiles[holding.Pile].Count;
                MakeMoveUsingFreeCells(move.From, holding.Index, holding.Pile);
                moveStack.Push(new Move(holding.Pile, undoFromIndex, move.To));
            }
            if (move.Type == MoveType.CompositeSinglePile)
            {
                // Composite single pile move.
                MakeCompositeSinglePileMove(move.Next);
            }
            else if (move.Type == MoveType.Swap)
            {
                // Swap move.
                SwapUsingFreeCells(move.From, move.FromIndex, move.To, move.ToIndex);
            }
            else
            {
                // Ordinary move.
                MakeMoveUsingFreeCells(move.From, move.FromIndex, move.To);
            }

            // Lastly move from the holding piles, if we still can.
            Analyze();
            int freeCells = FreeCells.Count;
            int maxExtraSuits = ExtraSuits(freeCells);
            while (moveStack.Count > 0)
            {
                Move undo = moveStack.Pop();
                int undoToIndex = UpPiles[undo.To].Count;
                if (undo.FromIndex >= UpPiles[undo.From].Count ||
                    undoToIndex != 0 && UpPiles[undo.From][undo.FromIndex].Face + 1 != UpPiles[undo.To][undoToIndex - 1].Face)
                {
                    // The pile has changed since we moved due to a discard.
#if false
                    Console.Clear();
                    PrintGames();
                    PrintMove(move);
                    Console.ReadKey();
#endif
                    break;
                }
                int extraSuits = CountSuits(undo.From, undo.FromIndex) - 1;
                if (extraSuits > maxExtraSuits)
                {
                    // The number of free cells has decreased due to the main move.
#if false
                    Console.Clear();
                    PrintGames();
                    PrintMove(move);
                    Console.ReadKey();
#endif
                    break;
                }
                MakeMoveUsingFreeCells(undo.From, undo.FromIndex, undo.To);
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

        private void SwapUsingFreeCells(int from, int fromIndex, int to, int toIndex)
        {
            if (Diagnostics)
            {
                Utils.WriteLine("SWUFC: {0}/{1} -> {2}/{3}", from, fromIndex, to, toIndex);
            }
            Analyze();
            int freeCells = FreeCells.Count;
            int fromSuits = CountSuits(from, fromIndex);
            int toSuits = CountSuits(to, toIndex);
            if (fromSuits + toSuits - 1 > ExtraSuits(freeCells))
            {
                throw new InvalidMoveException("insufficient free cells");
            }
            Stack<Move> moveStack = new Stack<Move>();
            for (int n = freeCells; n > 0 && fromSuits + toSuits > 1; n--)
            {
                if (fromSuits >= toSuits)
                {
                    int moveSuits = toSuits != 0 ? fromSuits : fromSuits - 1;
                    fromSuits -= MoveOffUsingFreeCells(from, fromIndex, to, moveSuits, n, moveStack);
                }
                else
                {
                    int moveSuits = fromSuits != 0 ? toSuits : toSuits - 1;
                    toSuits -= MoveOffUsingFreeCells(to, toIndex, from, moveSuits, n, moveStack);
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

        private void UnloadToFreeCells(int from, int lastFromIndex, int to, Stack<Move> moveStack)
        {
            if (Diagnostics)
            {
                Utils.WriteLine("ULTFC: {0}/{1} -> {2}", from, lastFromIndex, to);
            }
            Analyze();
            int freeCells = FreeCells.Count;
            int suits = CountSuits(from, lastFromIndex);
            if (suits > ExtraSuits(freeCells))
            {
                throw new InvalidMoveException("insufficient free cells");
            }
            int totalSuits = CountSuits(from, lastFromIndex);
            int remainingSuits = totalSuits;
            int fromIndex = UpPiles[from].Count;
            for (int n = 0; n < freeCells; n++)
            {
                int m = Math.Min(freeCells, n + remainingSuits);
                for (int i = m - 1; i >= n; i--)
                {
                    int runLength = GetRunUp(from, fromIndex);
                    fromIndex -= runLength;
                    fromIndex = Math.Max(fromIndex, lastFromIndex);
                    MakeSimpleMove(from, -runLength, FreeCells[i]);
                    moveStack.Push(new Move(FreeCells[i], -runLength, to));
                    remainingSuits--;
                }
                for (int i = n + 1; i < m; i++)
                {
                    int runLength = UpPiles[FreeCells[i]].Count;
                    MakeSimpleMove(FreeCells[i], -runLength, FreeCells[n]);
                    moveStack.Push(new Move(FreeCells[n], -runLength, FreeCells[i]));
                }
                if (remainingSuits == 0)
                {
                    break;
                }
            }
        }

        private int MoveOffUsingFreeCells(int from, int lastFromIndex, int to, int remainingSuits, int n, Stack<Move> moveStack)
        {
            int suits = Math.Min(remainingSuits, n);
            if (Diagnostics)
            {
                Utils.WriteLine("MOUFC: {0} -> {1}: {2}", from, to, suits);
            }
            for (int i = n - suits; i < n; i++)
            {
                // Move as much as possible but not too much.
                Pile fromPile = UpPiles[from];
                int fromIndex = fromPile.Count - GetRunUp(from, fromPile.Count);
                if (fromIndex < lastFromIndex)
                {
                    fromIndex = lastFromIndex;
                }
                int runLength = fromPile.Count - fromIndex;
                MakeSimpleMove(from, -runLength, FreeCells[i]);
                moveStack.Push(new Move(FreeCells[i], -runLength, to));
            }
            for (int i = n - 2; i >= n - suits; i--)
            {
                int runLength = UpPiles[FreeCells[i]].Count;
                MakeSimpleMove(FreeCells[i], -runLength, FreeCells[n - 1]);
                moveStack.Push(new Move(FreeCells[n - 1], -runLength, FreeCells[i]));
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
                Analyze();
                int freeCells = FreeCells.Count;
                Move move = Normalize(SupplementaryList[next]);
                if (move.Type == MoveType.Unload)
                {
                    offloadPile = move.To;
                    UnloadToFreeCells(move.From, move.FromIndex, -1, moveStack);
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
                    if (SimpleMoveIsValid(move))
                    {
                        try
                        {
                            MakeMoveUsingFreeCells(move.From, move.FromIndex, move.To);
                        }
                        catch (InvalidMoveException)
                        {
                            // The move appeared to be valid but the pile
                            // has changed due to a discard and the move
                            // is no longer possible.
                        }
                    }
                }
                else
                {
                    if (SimpleMoveIsValid(move))
                    {
                        MakeMoveUsingFreeCells(move.From, move.FromIndex, move.To);
                    }
                    else
                    {
                        // Things got messed up due to a discard.  There should
                        // be another pile with the same target.
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
                            MakeMoveUsingFreeCells(move.From, move.FromIndex, to);
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

        private void MakeMovesUsingFreeCells(int first)
        {
            for (int next = first; next != -1; next = SupplementaryList[next].Next)
            {
                Move move = SupplementaryList[next];
                MakeMoveUsingFreeCells(move.From, move.FromIndex, move.To);
            }
        }

        private void MakeMoveUsingFreeCells(int from, int lastFromIndex, int to)
        {
            if (lastFromIndex < 0)
            {
                lastFromIndex += UpPiles[from].Count;
            }
            if (Diagnostics)
            {
                Utils.WriteLine("MMUFC: {0}/{1} -> {2}", from, lastFromIndex, to);
            }
            Analyze();
            int toIndex = UpPiles[to].Count;
            int extraSuits = CountSuits(from, lastFromIndex) - 1;
            if (extraSuits < 0)
            {
                throw new InvalidMoveException("not a single run");
            }
            if (extraSuits == 0)
            {
                MakeSimpleMove(from, lastFromIndex, to);
                return;
            }
            if (toIndex == 0)
            {
                FreeCells.Remove(to);
            }
            int freeCells = FreeCells.Count;
            int maxExtraSuits = ExtraSuits(freeCells);
            if (extraSuits > maxExtraSuits)
            {
                throw new InvalidMoveException("insufficient free cells");
            }
            int suits = 0;
            int fromIndex = UpPiles[from].Count;
            Stack<Move> moveStack = new Stack<Move>();
            for (int n = freeCells; n > 0; n--)
            {
                for (int i = 0; i < n; i++)
                {
                    int runLength = GetRunUp(from, fromIndex);
                    fromIndex -= runLength;
                    MakeSimpleMove(from, -runLength, FreeCells[i]);
                    moveStack.Push(new Move(FreeCells[i], -runLength, to));
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
                    int runLength = UpPiles[FreeCells[i]].Count;
                    MakeSimpleMove(FreeCells[i], -runLength, FreeCells[n - 1]);
                    moveStack.Push(new Move(FreeCells[n - 1], -runLength, FreeCells[i]));
                }
            }
            MakeSimpleMove(from, lastFromIndex, to);
            while (moveStack.Count != 0)
            {
                Move move = moveStack.Pop();
                MakeSimpleMove(move.From, move.FromIndex, move.To);
            }
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
            Debug.Assert(CountSuits(from, fromIndex) == 1);
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

                int runLength = GetRunUp(i, pile.Count);
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
            foreach (Move move in Moves)
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

        private static string ToAsciiString(Pile[] rows)
        {
            string s = "";
            int n = rows.Length;
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

        private static string ToPrettyString(Pile[] rows)
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
