using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Spider
{
    public class Player
    {
        public double[] InitialCoefficients { get; set; }
        public int Coefficient { get; set; }
        public double[] Coefficients { get; set; }
        public bool TraceStartFinish { get; set; }
        public bool TraceDeals { get; set; }
        public bool TraceMoves { get; set; }
        public bool SimpleMoves { get; set; }
        public bool Diagnostics { get; set; }

        public bool ShowResults { get; set; }

        public int Threads { get; set; }
        public int Games { get; set; }
        public int Suits { get; set; }
        public int Seed { get; set; }

        public int Played { get { return played; } }
        public int Won { get { return won; } }
        public int Discards { get { return discards; } }
        public int Moves { get { return moves; } }
        public bool[] Results { get; private set; }
        public int[] Instances { get; private set; }

        private int currentSeed;
        private int played;
        private int won;
        private int discards;
        private int moves;
        private int instance;
        private Semaphore semaphore;
        private Queue<Game> gameQueue;

        public Player()
        {
            Game game = new Game();
            InitialCoefficients = game.Coefficients;
            Coefficients = InitialCoefficients;
            TraceStartFinish = game.TraceStartFinish;
            TraceDeals = game.TraceDeals;
            TraceMoves = game.TraceMoves;
            ShowResults = false;

#if true
            Games = 100000;
#else
            count = 10;
#endif

            Suits = 2;
#if true
            Seed = 0;
#else
            Random random = new Random();
            Seed = random.Next();
#endif
            Threads = -1;
        }

        public void Play()
        {
            PlayOneSet();

            if (ShowResults)
            {
                for (int i = 0; i < Instances.Length; i++)
                {
                    Console.Write(Instances[i]);
                }
                Console.WriteLine("");
                for (int i = 0; i < Results.Length; i++)
                {
                    Console.Write(Results[i] ? "W" : "-");
                }
                Console.WriteLine("");
            }

            Console.WriteLine("games played: {0}, games won: {1:G4}%, games with discards: {2:G4}%", Played, 100.0 * Won / Played, 100.0 * Discards / Played);
            Console.WriteLine("average moves: {0:G4}", (double)Moves / Played);

            if (Debugger.IsAttached)
            {
                Console.ReadKey();
            }
        }

        public void EvaluateCoefficient()
        {
            int iterations = 21;
            double factor = 2;
            double initialValue = InitialCoefficients[Coefficient];
            double minValue = initialValue / factor;
            double maxValue = initialValue * factor;
            double multipler = Math.Exp((Math.Log(Math.Abs(maxValue)) - Math.Log(Math.Abs(minValue))) / (iterations - 1));
            double value = minValue;
            for (int i = 0; i < iterations; i++)
            {
                Coefficients[Coefficient] = value;
                PlayOneSet();
                double percentage = 100.0 * won / played;
                Console.WriteLine("Coefficient[{0}] = {1,-8:G5} {2,-8:G5}", Coefficient, value, percentage);
                value *= multipler;
            }
        }

        public void Compare()
        {
            while (true)
            {
                PlayOneSet();
                int won1 = Won;
                bool[] results1 = Results;
                int[] instances1 = Instances;
                PlayOneSet();
                int won2 = Won;
                bool[] results2 = Results;
                int[] instances2 = Instances;
                if (won1 != won2)
                {
                    for (int i = 0; i < results1.Length; i++)
                    {
                        Console.WriteLine("Game: {0}, Seed: {1}, Instance: {2}/{3}, Won: {4}/{5}",
                            i, Seed + i, instances1[i], instances2[i], results1[i] ? 1 : 0, results2[i] ? 1 : 0);
                    }
                    break;
                }
            }
        }

        public void PlayOneSet()
        {
            played = 0;
            won = 0;
            discards = 0;
            moves = 0;
            instance = 0;
            Results = new bool[Games];
            Instances = new int[Games];
            currentSeed = Seed;
            int threads = Threads;
            if (threads == -1)
            {
                threads = Environment.ProcessorCount;
            }
            if (Debugger.IsAttached)
            {
                threads = 1;
            }
            if (threads == 1)
            {
                Game game = new Game();
                game.Instance = instance;
                for (int i = 0; i < Games; i++)
                {
                    PlayOneGame(game);
                }
            }
            else
            {
                semaphore = new Semaphore(threads, threads);
                WaitCallback callback = new WaitCallback(ThreadPlayOneGame);
                gameQueue = new Queue<Game>();

                for (int i = 0; i < Games; i++)
                {
                    // Wait for a processor to become available.
                    semaphore.WaitOne();

                    // Queue the work item to a thread.
                    ThreadPool.QueueUserWorkItem(callback, null);

                }

                //  Allow all threads to finish.
                for (int i = 0; i < threads; i++)
                {
                    semaphore.WaitOne();
                }
            }
        }

        private void PlayOneGame(Game game)
        {
            game.TraceStartFinish = TraceStartFinish;
            game.TraceDeals = TraceDeals;
            game.TraceMoves = TraceMoves;
            game.SimpleMoves = SimpleMoves;
            game.Diagnostics = Diagnostics;

            game.Coefficients = Coefficients;
            game.Suits = Suits;
            game.Seed = Interlocked.Increment(ref currentSeed) - 1;
            game.Play();
            if (game.Won)
            {
                Interlocked.Increment(ref won);
            }
            if (game.DiscardPiles.Count > 0)
            {
                Interlocked.Increment(ref discards);
            }
            Interlocked.Increment(ref played);
            Interlocked.Add(ref moves, game.Moves.Count);
            Results[game.Seed - Seed] = game.Won;
            Instances[game.Seed - Seed] = game.Instance;
        }

        private void ThreadPlayOneGame(object state)
        {
            Game game = GetGame();
            PlayOneGame(game);
            ReleaseGame(game);
            semaphore.Release();
        }

        private Game GetGame()
        {
            lock (gameQueue)
            {
                if (gameQueue.Count == 0)
                {
                    Game game = new Game();
                    game.Instance = instance++;
                    return game;
                }
                return gameQueue.Dequeue();
            }
        }

        private void ReleaseGame(Game game)
        {
            lock (gameQueue)
            {
                gameQueue.Enqueue(game);
            }
        }
    }
}
