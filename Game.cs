#define VALIDATE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    public class Game
    {
        public const int NumberOfPiles = 10;
        public const int MaximumMoves = 1000;
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
        public bool SimpleMoves { get; set; }
        public bool Diagnostics { get; set; }

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
        private int[] RunLengths { get; set; }
        private PileList FreeCells { get; set; }
        private PileList[] FaceLists { get; set; }

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
            Diagnostics = false;

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
            RunLengths = new int[NumberOfPiles];
            FreeCells = new PileList();
            FaceLists = new PileList[(int)(Face.King + 1)];
            for (int i = (int)Face.Ace; i <= (int)Face.King; i++)
            {
                FaceLists[i] = new PileList();
            }
            //Coefficients = new double[] { 100, 10, 1, 90, 10, 1, -1 };
            //Coefficients = new double[] { 100, 6.6, 1, 90, 23.7, 6.6, -1 };
#if false
            Coefficients = new double[] { 6.6, 0, 1, 90, 0, 23.7, 6.6, 0, -1 };
#else
            Coefficients = new double[] { 8.9835, 9.3244, -0.076368 };
#endif
        }

        public void Play()
        {
            Clear();
            Start();
            if (TraceStartFinish)
            {
                PrintGame();
            }
            while (true)
            {
                if (Moves.Count == MaximumMoves)
                {
                    if (TraceStartFinish)
                    {
                        PrintGame();
                        Console.WriteLine("maximum moves exceeded");
                    }
                    break;
                }
                if (!Move())
                {
                    if (StockPile.Count > 0)
                    {
                        if (TraceDeals)
                        {
                            PrintGame();
                            Console.WriteLine("dealing");
                        }
                        Deal();
                        continue;
                    }
                    if (TraceStartFinish)
                    {
                        PrintGame();
                        Console.WriteLine("lost - no moves");
                    }
                    break;
                }
                if (Won)
                {
                    if (TraceStartFinish)
                    {
                        PrintGame();
                        Console.WriteLine("won");
                    }
                    break;
                }
                Discard();
                ExposeCards();
            }
        }

        public void Clear()
        {
            Won = false;
            Moves.Clear();
            Shuffled.Clear();
            StockPile.Clear();
            for (int i = 0; i < NumberOfPiles; i++)
            {
                DownPiles[i].Clear();
                UpPiles[i].Clear();
            }
            DiscardPiles.Clear();
        }

        public void Start()
        {
            if (Suits == 1)
            {
                Deck = OneSuitDeck;
            }
            else if (Suits == 2)
            {
                Deck = TwoSuitDeck;
            }
            else if (Suits == 4)
            {
                Deck = FourSuitDeck;
            }
            else
            {
                throw new Exception("Invalid number of suits");
            }

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

        private void Deal()
        {
            for (int i = 0; i < NumberOfPiles; i++)
            {
                UpPiles[i].Add(StockPile.Next());
            }
            GetRunLengths();
        }

        public bool Move()
        {
            Candidates.Clear();
            SupplementaryMoves.Clear();
            Analyze();

            if (FreeCells.Count == NumberOfPiles)
            {
                Won = true;
                return true;
            }

            // The formula for how many intermediate runs can
            // be moved is m: = sum(1 + 2 + ... + n).
            int freeCells = FreeCells.Count;
            int baseMaxExtraSuits = freeCells * (freeCells + 1) / 2;

            for (int from = 0; from < NumberOfPiles; from++)
            {
                int holdingPile = -1;
                int holdingPileCandidate = -1;
                int holdingPileSuits = 0;
                int holdingPileIndex = 0;
                Pile fromPile = UpPiles[from];
                int extraSuits = 0;
                int maxExtraSuits = baseMaxExtraSuits;
                int runLength = 0;
                RunLengths[from] = 0;
                for (int fromIndex = fromPile.Count - 1; fromIndex >= 0; fromIndex--)
                {
                    Card fromCard = fromPile[fromIndex];
                    if (fromIndex < fromPile.Count - 1)
                    {
                        Card previousCard = fromPile[fromIndex + 1];
                        if (previousCard.Face + 1 != fromCard.Face)
                        {
                            // Check for swap with a whole pile.
                            CheckWholePileSwap(from, fromIndex + 1, extraSuits, maxExtraSuits, holdingPile, holdingPileIndex);
                            break;
                        }
                        if (fromCard.Suit != previousCard.Suit)
                        {
                            // This is a cross-suit run.
                            extraSuits++;
                            runLength = 0;
                            if (extraSuits > maxExtraSuits)
                            {
                                if (holdingPile != -1 || holdingPileCandidate == -1)
                                {
                                    break;
                                }
                                if (extraSuits <= maxExtraSuits + holdingPileSuits)
                                {
                                    holdingPile = holdingPileCandidate;
                                    maxExtraSuits += holdingPileSuits;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                    runLength++;
                    if (extraSuits == 0)
                    {
                        RunLengths[from] = runLength;
                    }

                    // Add moves to other piles.
                    if (fromCard.Face < Face.King)
                    {
                        PileList piles = FaceLists[(int)fromCard.Face + 1];
                        for (int i = 0; i < piles.Count; i++)
                        {
                            int to = piles[i];
                            if (from == to || from == holdingPile)
                            {
                                continue;
                            }

                            // We've found a legal move.
                            Pile toPile = UpPiles[to];
                            Candidates.Add(new Move(from, fromIndex, to, toPile.Count, holdingPile, holdingPileIndex));

                            // Update the best holding pile move.
                            if (holdingPile == -1 && extraSuits > holdingPileSuits)
                            {
                                holdingPileCandidate = to;
                                holdingPileIndex = fromIndex;
                                holdingPileSuits = extraSuits;
                            }
                        }
                    }

                    // Cannot do a composite move to
                    // a free cell if there are not
                    // enough other free cells.
                    if (extraSuits <= maxExtraSuits - freeCells)
                    {
                        // Add moves to a free cell.
                        for (int i = 0; i < FreeCells.Count; i++)
                        {
                            int to = FreeCells[0];

                            if (fromIndex == 0)
                            {
                                // No point in moving from a full pile
                                // from one open position to another unless
                                // there are more cards to expose.
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

                            // We've found a legal move.
                            Pile toPile = UpPiles[to];
                            Candidates.Add(new Move(from, fromIndex, to, toPile.Count, holdingPile, holdingPileIndex));

                            // Only need to check the first free cell
                            // since all free cells are the same
                            // except for undealt cards.
                            break;
                        }
                    }

                    // Check for swaps.
                    CheckSwaps(from, fromIndex, extraSuits, maxExtraSuits, holdingPile, holdingPileIndex);
                }

                // Check for buried free-cell preserving moves.
                CheckBuried(from, fromPile, maxExtraSuits, holdingPile, holdingPileIndex);
            }

            return ChooseMove();
        }

        private void CheckWholePileSwap(int from, int fromIndex, int extraSuits, int maxExtraSuits, int holdingPile, int holdingPileIndex)
        {
            Pile fromPile = UpPiles[from];
            Card exposedCard = fromPile[fromIndex - 1];
            for (int to = 0; to < NumberOfPiles; to++)
            {
                if (to == from || to == holdingPile)
                {
                    continue;
                }
                Pile toPile = UpPiles[to];
                if (toPile.Count == 0 || DownPiles[to].Count != 0)
                {
                    continue;
                }
                Card rootCard = toPile[0];
                if (exposedCard.Face - 1 != rootCard.Face)
                {
                    continue;
                }
                int toSuits = CountSuits(to, 0);
                if (toSuits == -1 || extraSuits + toSuits > maxExtraSuits)
                {
                    continue;
                }
                Candidates.Add(new Move(from, fromIndex, to, 0, holdingPile, holdingPileIndex));
            }
        }

        private void CheckSwaps(int from, int fromIndex, int extraSuits, int maxExtraSuits, int holdingPile, int holdingPileIndex)
        {
            // Look for swap moves.
            if (fromIndex == 0 && DownPiles[from].Count != 0)
            {
                return;
            }
            Pile fromPile = UpPiles[from];
            Card fromCard = fromPile[fromIndex];
            Card fromCardParent = Card.Empty;
            if (fromIndex != 0)
            {
                fromCardParent = fromPile[fromIndex - 1];
            }
            for (int to = 0; to < NumberOfPiles; to++)
            {
                if (to == from || to == holdingPile)
                {
                    continue;
                }
                Pile toPile = UpPiles[to];
                int toIndex = toPile.Count;
                int extraSuitsToLimit = maxExtraSuits - extraSuits;
                for (int extraSuitsTo = 0; extraSuitsTo < extraSuitsToLimit; extraSuitsTo++)
                {
                    if (toIndex == 0)
                    {
                        break;
                    }
#if false
                    if (toPile[toIndex - 1].Face > fromCardParent.Face)
                    {
                        // Cannot possibly lead up to our card.
                        break;
                    }
#endif

                    int toRun = GetRunUp(to, toIndex - 1);
                    toIndex = toIndex - toRun;
                    if (toIndex == 0)
                    {
                        // No longer a target.
                        break;
                    }
                    Card toCard = toPile[toIndex - 1];
                    Card toCardChild = toPile[toIndex];
#if true
                    if (toCard.Face - 1 == fromCard.Face && fromCardParent.Face - 1 == toCardChild.Face)
                    {
                        // We've found a legal move.
                        Candidates.Add(new Move(from, fromIndex, to, toIndex, holdingPile, holdingPileIndex));
                    }
                    if (toCard.Face - 1 != toCardChild.Face)
                    {
                        // No longer a continuous run.
                        break;
                    }
#else
                    if (toCard.Face - 1 != toPile[toIndex].Face)
                    {
                        // No longer a continuous run.
                        break;
                    }
                    if (toCard.Suit == fromCard.Suit && toCard.Face - 1 == fromCard.Face)
                    {
                        // We've found a legal move.
                        Candidates.Add(new Move(from, fromIndex, to, toIndex, holdingPile, holdingPileIndex));
                    }
#endif
                }
            }
        }

        private void CheckBuried(int from, Pile fromPile, int maxExtraSuits, int holdingPile, int holdingPileIndex)
        {
            // Check for buried sequences that can be uncovered.
            if (DownPiles[from].Count != 0 || fromPile.Count == 0)
            {
                return;
            }
            Card fromCard = fromPile[0];
            if (fromCard.Face == Face.King)
            {
                return;
            }
            bool canReach = false;
            int fromIndex = fromPile.Count;
            int fromRun = GetRunUp(from, fromIndex - 1);
            fromIndex = fromIndex - fromRun;
            if (fromIndex == 0)
            {
                return;
            }
            for (int extraSuitsFrom = 0; extraSuitsFrom < maxExtraSuits; extraSuitsFrom++)
            {
                if (fromPile[fromIndex - 1].Face - 1 != fromPile[fromIndex].Face)
                {
                    // No longer a continuous run.
                    break;
                }
                fromRun = GetRunUp(from, fromIndex - 1);
                if (fromIndex - fromRun == 0)
                {
                    canReach = true;
                    break;
                }
                fromIndex = fromIndex - fromRun;
            }
            if (!canReach)
            {
                // Can't reach the buried sequence.
                return;
            }
            if (fromPile[fromIndex - 1].Face - 1 == fromPile[fromIndex].Face)
            {
                // The pile is single continuous sequence.
                return;
            }
            PileList piles = FaceLists[(int)fromCard.Face + 1];
            for (int i = 0; i < piles.Count; i++)
            {
                int to = piles[i];
                if (to == from || to == holdingPile)
                {
                    continue;
                }
                Pile toPile = UpPiles[to];

                Debug.Assert(fromPile.Count > 0);
                Debug.Assert(fromIndex > 0);
                Debug.Assert(fromIndex < fromPile.Count);
                Debug.Assert(toPile.Count > 0);
                Debug.Assert(CountSuits(from, 0, fromIndex) == 1);
                Debug.Assert(CountSuits(from, fromIndex) >= 1);
                Debug.Assert(fromPile[fromIndex - 1].Face - 1 != fromPile[fromIndex].Face);
                Debug.Assert(fromPile[0].Face + 1 == toPile[toPile.Count - 1].Face);

                // Add the first uncovering move.
                Candidates.Add(new Move(from, fromIndex, FreeCells[0], 0, SupplementaryMoves.Count));

                // Add the supplementary move.
                SupplementaryMoves.Add(new Move(from, 0, to, toPile.Count));
            }
        }

        private void Analyze()
        {
            FreeCells.Clear();
            for (int i = (int)Face.Ace; i <= (int)Face.King; i++)
            {
                FaceLists[i].Clear();
            }

            // Prepare free cells and face lists.
            for (int i = 0; i < NumberOfPiles; i++)
            {
                Pile pile = UpPiles[i];
                if (pile.Count == 0)
                {
                    FreeCells.Add(i);
                }
                else
                {
                    FaceLists[(int)pile[pile.Count - 1].Face].Add(i);
                }
            }
        }

        private double CalculateScore(Move move)
        {
            if (move.Next != -1)
            {
                return InfiniteScore;
            }
            int from = move.From;
            int fromIndex = move.FromIndex;
            int to = move.To;
            int toIndex = move.ToIndex;

#if false
            if (Moves.Count == 87 && from == 3 && to == 4)
            {
                Console.WriteLine("Calculating zero score");
            }
#endif

            Pile fromPile = UpPiles[from];
            Pile toPile = UpPiles[to];
            Card fromCard = fromPile[fromIndex];
            if (toPile.Count == 0)
            {
                double lastResortScore = 1;
                if (fromIndex > 0)
                {
                    Card exposedCard = fromPile[fromIndex - 1];
                    if (exposedCard.Face == Face.Ace)
                    {
                        lastResortScore = 0;
                    }
                    else if (fromPile[fromIndex - 1].Face - 1 != fromCard.Face)
                    {
                        // Check whether the exposed card will be useful.
                        int maxExtraSuits = FreeCells.Count * (FreeCells.Count + 1) / 2;
                        int fromSuits = CountSuits(from, fromIndex);
                        for (int nextFrom = 0; nextFrom < NumberOfPiles; nextFrom++)
                        {
                            if (nextFrom == from || nextFrom == to)
                            {
                                // Inappropriate column.
                                continue;
                            }
                            Pile nextFromPile = UpPiles[nextFrom];
                            int nextFromIndex = nextFromPile.Count;
                            if (nextFromIndex == 0)
                            {
                                // Column is empty.
                                continue;
                            }
                            for (int extraSuits = fromSuits; extraSuits <= maxExtraSuits; extraSuits++)
                            {
                                int nextFromRun = GetRunUp(nextFrom, nextFromIndex - 1);
                                nextFromIndex = nextFromIndex - nextFromRun;
                                if (nextFromIndex == 0)
                                {
                                    // Card has no next to expose.
                                    break;
                                }
                                Card nextFromCard = nextFromPile[nextFromIndex];
                                if (nextFromCard.Face + 1 == nextFromPile[nextFromIndex - 1].Face)
                                {
                                    // Card is already on its successor.
                                    continue;
                                }
                                if (nextFromCard.Face + 1 != exposedCard.Face)
                                {
                                    // Card isn't the card we need.
                                    break;
                                }

                                // Card leads to additional useful moves.
                                lastResortScore++;

                                // Try to find a target for the rest of the pile.
                                int restFromIndex = 0;
                                Card restFromCard = nextFromPile[restFromIndex];
                                int restSuits = CountSuits(nextFrom, 0, nextFromIndex);
                                if (restSuits == -1)
                                {
                                    // The rest isn't a single run.
                                    break;
                                }
                                if (extraSuits + restSuits > maxExtraSuits)
                                {
                                    // Not enough free cells.
                                    break;
                                }

                                for (int restTo = 0; restTo < NumberOfPiles; restTo++)
                                {
                                    if (restTo == to)
                                    {
                                        // Inappropriate column.
                                        continue;
                                    }
                                    Pile restToPile = UpPiles[restTo];
                                    int restToIndex = restToPile.Count;
                                    if (restToIndex == 0)
                                    {
                                        // Column is empty.
                                        continue;
                                    }
                                    Card restToCard = restToPile[restToIndex - 1];
                                    if (restToCard.Face - 1 != restFromCard.Face)
                                    {
                                        // Card isn't the card we need.
                                        continue;
                                    }

                                    // Found a home for the rest.
                                    lastResortScore = InfiniteScore;
#if false
                                    PrintMove(move);
                                    Console.WriteLine("next move: {0}", new Move(nextFrom, nextFromIndex, from, fromIndex));
                                    Console.WriteLine("rest move: {0}", new Move(nextFrom, restFromIndex, restTo, restToIndex));
                                    PrintGame();
                                    Debugger.Break();
#endif

                                    break;
                                }

                                break;
                            }
                        }
                    }
                }
                else
                {
                    // Prefer to move entire piles that
                    // are more likely to become free cells.
                    lastResortScore += 5 - DownPiles[from].Count;
                }

#if false
                // Don't move to an empty pile unless we
                // are out of stock.
                if (StockPile.Count > 0)
                {
                    return RejectScore;
                }
#endif

                if (fromIndex == 0)
                {
                    // Only move an entire pile if there
                    // are more cards to be exposed.
                    if (DownPiles[from].Count > 0)
                    {
                        return lastResortScore;
                    }
                    return RejectScore;
                }

                if (fromPile[fromIndex - 1].Face - 1 != fromCard.Face)
                {
                    // This exposes a non-consecutive card.
                    return lastResortScore;
                }

                // No point in splitting consecutive cards
                // unless they are part of a multi-move
                // sequence.
                return RejectScore;
            }

            int faceValue = (int)fromCard.Face;
            int wholePile = fromIndex == 0 ? 1 : 0;
            int moveRun = GetRunDown(from, fromIndex);
            int fromRun = GetRunUp(from, fromIndex) + moveRun - 1;
            int toRun = toIndex > 0 ? GetRunUp(to, toIndex - 1) : 0;
            int joinsTo = toIndex > 0 && fromCard.Suit == toPile[toIndex - 1].Suit ? 1 : 0;
            int splitsFrom = moveRun != fromRun ? 1 : 0;
            int downCount = fromIndex == 0 ? DownPiles[from].Count : 0;
            int runLength = 0;
            if (joinsTo != 0)
            {
                if (splitsFrom != 0)
                {
                    int oldMax = Math.Max(fromRun, toRun);
                    int newFromMax = Math.Max(moveRun, fromRun - moveRun);
                    int newMax = Math.Max(newFromMax, moveRun + toRun);
                    if (newMax > oldMax)
                    {
                        runLength = newMax;
                    }
                    else
                        return RejectScore;
                }
                else
                {
                    runLength = moveRun + toRun;
                }
            }
            else
            {
                splitsFrom = 0;
                if (fromIndex != 0)
                {
                    Card nextCard = fromPile[fromIndex - 1];
                    if (nextCard.Face - 1 == fromCard.Face)
                    {
                        splitsFrom = 1;

                        // Prefer to leave longer runs exposed.
                        if (toIndex == toPile.Count &&
                            fromIndex > 0 &&
                            fromCard.Suit != fromPile[fromIndex - 1].Suit)
                        {
                            int nextFromRun = GetRunUp(from, fromIndex - 1);
                            int nextToRun = GetRunUp(to, toIndex - 1);
                            if (nextFromRun <= nextToRun)
                            {
                                // The other position is better.
                                return RejectScore;
                            }

                            // Break the tie.
                            splitsFrom = 0;
                        }

                    }
                }
            }

            // Reject moves that are not a net advantange.
            if (joinsTo == 0 && splitsFrom != 0)
            {
                return RejectScore;
            }

#if false
            double score = 100 +
                Coefficients[0] * faceValue +
                Coefficients[1] * runLength +
                Coefficients[2] * faceValue * runLength +
                Coefficients[3] * joinsTo * faceValue +
                Coefficients[4] * joinsTo * runLength +
                Coefficients[5] * joinsTo * faceValue * runLength +
                Coefficients[6] * wholePile +
                Coefficients[7] * downCount +
                Coefficients[8] * wholePile * downCount;
#else
            double score = 100000 + faceValue +
                Coefficients[0] * runLength +
                Coefficients[1] * wholePile +
                Coefficients[2] * wholePile * downCount;
#endif

            return score;
        }

        private void GetRunLengths()
        {
#if true
            for (int i = 0; i < NumberOfPiles; i++)
            {
                Pile pile = UpPiles[i];
                if (pile.Count == 0)
                {
                    RunLengths[i] = 0;
                }
                else
                {
                    RunLengths[i] = -1;
                    RunLengths[i] = GetRunUp(i, pile.Count - 1);
                }
            }
#endif
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
#if true
            // Use cached information if applicable.
            int cachedRunLength = RunLengths[column];
            if (cachedRunLength != -1)
            {
                Pile pile = UpPiles[column];
                int runStart = pile.Count - cachedRunLength;
                if (row >= runStart)
                {
                    Debug.Assert(GetRunUpSlow(column, row) == row - runStart + 1);
                    return row - runStart + 1;
                }
            }
#endif

            return GetRunUpSlow(column, row);
        }

        private int GetRunUpSlow(int column, int row)
        {
            Pile pile = UpPiles[column];
            int runLength = 1;
            for (int index = row - 1; index >= 0; index--)
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

        private int GetRunDown(int column, int row)
        {
#if true
            // Use cached information if applicable.
            int cachedRunLength = RunLengths[column];
            if (cachedRunLength != -1)
            {
                Pile pile = UpPiles[column];
                int runStart = pile.Count - cachedRunLength;
                if (row >= runStart)
                {
                    Debug.Assert(GetRunDownSlow(column, row) == pile.Count - row);
                    return pile.Count - row;
                }
            }
#endif
            return GetRunDownSlow(column, row);
        }

        private int GetRunDownSlow(int column, int row)
        {
            Pile pile = UpPiles[column];
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
                PrintMoves();
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

            if (!SimpleMoves)
            {
                MakeMove(move);
                return true;
            }

            while (true)
            {
                int fromHoldingPileIndex = 0;
                if (move.HoldingPile != -1)
                {
                    // First move to the holding pile.
                    fromHoldingPileIndex = UpPiles[move.HoldingPile].Count;
                    MakeMoveUsingFreeCells(move.From, move.HoldingPileIndex, move.HoldingPile);
                }
                if (move.ToIndex != UpPiles[move.To].Count)
                {
                    // Swap move.
                    SwapUsingFreeCells(move.From, move.FromIndex, move.To, move.ToIndex);
                }
                else
                {
                    // Ordinary move.
                    MakeMoveUsingFreeCells(move.From, move.FromIndex, move.To);
                }
                if (move.HoldingPile != -1)
                {
                    Analyze();
                    int freeCells = FreeCells.Count;
                    int maxExtraSuits = freeCells * (freeCells + 1) / 2;
                    int extraSuits = CountSuits(move.HoldingPile, fromHoldingPileIndex) - 1;
                    if (extraSuits <= maxExtraSuits)
                    {
                        // Lastly move from the holding pile, if we still can.
                        MakeMoveUsingFreeCells(move.HoldingPile, fromHoldingPileIndex, move.To);
                    }
                }
                if (move.Next == -1)
                {
                    break;
                }
                move = SupplementaryMoves[move.Next];
            }
            return true;
        }

        private void SwapUsingFreeCells(int from, int fromIndex, int to, int toIndex)
        {
            if (Diagnostics)
            {
                Console.WriteLine("SWUFC: {0}/{1} -> {2}/{3}", from, fromIndex, to, toIndex);
            }
            Analyze();
            int freeCells = FreeCells.Count;
            int fromSuits = CountSuits(from, fromIndex);
            int toSuits = CountSuits(to, toIndex);
            Debug.Assert(fromSuits + toSuits - 1 <= freeCells * (freeCells + 1) / 2);
            Stack<Move> moveStack = new Stack<Move>();
            for (int n = freeCells; fromSuits + toSuits > 1; n--)
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

        private int MoveOffUsingFreeCells(int from, int lastFromIndex, int to, int remainingSuits, int n, Stack<Move> moveStack)
        {
            int suits = Math.Min(remainingSuits, n);
            if (Diagnostics)
            {
                Console.WriteLine("MOUFC: {0} -> {1}: {2}", from, to, suits);
            }
            for (int i = n - suits; i < n; i++)
            {
                // Move as much as possible but not too much.
                Pile fromPile = UpPiles[from];
                int fromIndex = fromPile.Count - GetRunUp(from, fromPile.Count - 1);
                if (fromIndex < lastFromIndex)
                {
                    fromIndex = lastFromIndex;
                }
                MakeSimpleMove(from, fromIndex, FreeCells[i]);
                moveStack.Push(new Move(FreeCells[i], -1, to, -1));
            }
            for (int i = n - 2; i >= n - suits; i--)
            {
                MakeSimpleMove(FreeCells[i], -1, FreeCells[n - 1]);
                moveStack.Push(new Move(FreeCells[n - 1], -1, FreeCells[i], -1));
            }
            return suits;
        }

        private void MakeMoveUsingFreeCells(int from, int fromIndex, int to)
        {
            if (Diagnostics)
            {
                Console.WriteLine("MMUFC: {0}/{1} -> {2}", from, fromIndex, to);
            }
            Analyze();
            int toIndex = UpPiles[to].Count;
            int extraSuits = CountSuits(from, fromIndex) - 1;
            if (extraSuits == 0)
            {
                MakeSimpleMove(from, fromIndex, to);
                return;
            }
            int nextFromIndex = fromIndex + GetRunDown(from, fromIndex);
            int toFreeCell = to;
            if (toIndex == 0)
            {
                FreeCells.Remove(to);
            }
            int freeCells = FreeCells.Count;
            int maxExtraSuits = freeCells * (freeCells + 1) / 2;
            if (extraSuits > maxExtraSuits)
            {
                PrintGame();
                throw new Exception("insufficient free cells for move");
            }
            int suits = 0;
            Stack<Move> moveStack = new Stack<Move>();
            for (int n = freeCells; n > 0; n--)
            {
                for (int i = 0; i < n; i++)
                {
                    MakeSimpleMove(from, -1, FreeCells[i]);
                    moveStack.Push(new Move(FreeCells[i], -1, to, -1));
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
                    MakeSimpleMove(FreeCells[i], -1, FreeCells[n - 1]);
                    moveStack.Push(new Move(FreeCells[n - 1], -1, FreeCells[i], -1));
                }
            }
            MakeSimpleMove(from, fromIndex, to);
            while (moveStack.Count != 0)
            {
                Move move = moveStack.Pop();
                MakeSimpleMove(move.From, move.FromIndex, move.To);
            }
        }

        private void MakeSimpleMove(int from, int fromIndex, int to)
        {
            if (fromIndex == -1)
            {
                // If from is not supplied move as much as possible.
                Pile fromPile = UpPiles[from];
                fromIndex = fromPile.Count - GetRunUp(from, fromPile.Count - 1);
            }
            Debug.Assert(UpPiles[from].Count != 0);
            Debug.Assert(fromIndex < UpPiles[from].Count);
            Debug.Assert(CountSuits(from, fromIndex) == 1);
            Debug.Assert(UpPiles[to].Count == 0 || UpPiles[from][fromIndex].Face + 1 == UpPiles[to][UpPiles[to].Count - 1].Face);
            if (Diagnostics)
            {
                Console.WriteLine("    MSM: {0}/{1} -> {2}", from, fromIndex, to);
            }
            MakeMove(new Move(from, fromIndex, to, UpPiles[to].Count));
        }

        private void MakeMove(Move move)
        {
            // Make the moves.
            while (true)
            {
                Pile fromPile = UpPiles[move.From];
                Pile toPile = UpPiles[move.To];
                int fromIndex = move.FromIndex;
                int fromCount = fromPile.Count - fromIndex;
                if (move.ToIndex != toPile.Count)
                {
                    int toIndex = move.ToIndex;
                    int toCount = toPile.Count - toIndex;
                    ScratchPile.AddRange(toPile, toIndex, toCount);
                    toPile.RemoveRange(toIndex, toCount);
                    toPile.AddRange(fromPile, fromIndex, fromCount);
                    fromPile.RemoveRange(fromIndex, fromCount);
                    fromPile.AddRange(ScratchPile, 0, toCount);
                    ScratchPile.Clear();
                }
                else
                {
                    toPile.AddRange(fromPile, fromIndex, fromCount);
                    fromPile.RemoveRange(fromIndex, fromCount);
                }
                RunLengths[move.From] = -1;
                RunLengths[move.To] = -1;
                Moves.Add(move);
                if (move.Next == -1)
                {
                    break;
                }
                move = SupplementaryMoves[move.Next];
            }
            if (TraceMoves)
            {
                Console.WriteLine("Move {0}: {1}", Moves.Count, move);
            }
        }

        private void PrintMoves()
        {
            Console.WriteLine("==");
            foreach (Move move in Candidates)
            {
                Console.WriteLine("{0}", move);
                for (int next = move.Next; next != -1; next = SupplementaryMoves[next].Next)
                {
                    Move nextMove = SupplementaryMoves[move.Next];
                    Console.WriteLine("    {0}", nextMove);
                }
            }
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

                int runLength = GetRunUp(i, pile.Count - 1);
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
                    RunLengths[i] = -1;
                }
            }
        }

        private void ExposeCards()
        {
            for (int i = 0; i < NumberOfPiles; i++)
            {
                Pile up = UpPiles[i];
                Pile down = DownPiles[i];
                if (up.Count == 0 && down.Count > 0)
                {
                    up.Add(down.Next());
                    RunLengths[i] = -1;
                }
            }
        }

        public void PrintGame()
        {
            Utils.ColorizeToConsole(ToString());
        }

        public void PrintMove(Move move)
        {
            Console.WriteLine(move);
        }

        public override string ToString()
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
            s += ToString(-1, discardRow);
            s += Environment.NewLine;
            s += ToString(DownPiles);
            s += Environment.NewLine;
            s += "   0  1  2  3  4  5  6  7  8  9";
            s += Environment.NewLine;
            s += ToString(UpPiles);
            s += Environment.NewLine;
            for (int i = 0; i < StockPile.Count / NumberOfPiles; i++)
            {
                Pile row = new Pile();
                for (int j = 0; j < NumberOfPiles; j++)
                {
                    row.Add(StockPile[i * NumberOfPiles + j]);
                }
                s += ToString(i, row);
            }

            return s;
        }

        private static string ToString(Pile[] rows)
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
                s += ToString(j, row);
            }
            return s;
        }

        private static string ToString(int index, Pile row)
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
    }
}
