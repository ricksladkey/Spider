using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using NumUtils.NelderMeadSimplex;
using LevenbergMarquardtLeastSquaresFitting;

using Spider.Collections;
using Spider.Engine;

namespace Spider.GamePlay
{
    public class Player : IGameSettings
    {
        #region IGameSettings Members

        public Variation Variation { get; set; }
        public int Seed { get; set; }
        public double[] Coefficients { get; set; }
        public bool Diagnostics { get; set; }
        public bool Interactive { get; set; }
        public int Instance { get; set; }
        public bool UseSearch { get; set; }

        public bool TraceMoves { get; set; }
        public bool TraceStartFinish { get; set; }
        public bool TraceDeals { get; set; }
        public bool TraceSearch { get; set; }
        public bool ComplexMoves { get; set; }

        #endregion

        public double[] InitialCoefficients { get; set; }
        public int Coefficient { get; set; }

        public bool ShowResults { get; set; }
        public bool Profile { get; set; }

        public int Threads { get; set; }
        public int Games { get; set; }

        public int Played { get { return played; } }
        public int Won { get { return won; } }
        public int Discards { get { return discards; } }
        public int Moves { get { return moves; } }
        public int MovesWon { get { return movesWon; } }
        public int MovesLost { get { return movesLost; } }
        public bool[] Results { get; private set; }
        public int[] Instances { get; private set; }

        private int currentSeed;
        private int played;
        private int won;
        private int discards;
        private int moves;
        private int movesWon;
        private int movesLost;
        private int instance;
        private Semaphore semaphore;
        private Queue<Game> gameQueue;

        public Player()
        {
            Game game = new Game();
            TraceStartFinish = game.TraceStartFinish;
            TraceDeals = game.TraceDeals;
            TraceMoves = game.TraceMoves;
            TraceSearch = game.TraceSearch;
            ComplexMoves = game.ComplexMoves;
            UseSearch = game.UseSearch;
            ShowResults = false;

            Games = 100000;
            Variation = Variation.Spider2;
            Seed = 0;
            Threads = -1;
        }

        private void PlayOneGame(Game game)
        {
            game.TraceStartFinish = TraceStartFinish;
            game.TraceDeals = TraceDeals;
            game.TraceMoves = TraceMoves;
            game.TraceSearch = TraceSearch;
            game.ComplexMoves = ComplexMoves;
            game.UseSearch = UseSearch;
            game.Diagnostics = Diagnostics;
            game.Interactive = Interactive;

            game.Coefficients = Coefficients;
            game.Variation = Variation;
            game.Seed = Interlocked.Increment(ref currentSeed) - 1;

            if (Profile)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                game.Play();
                long elapsed = stopwatch.ElapsedMilliseconds;
                Console.WriteLine("seed = {0,6}, elapsed = {1,6}", game.Seed, elapsed);
            }
            else
            {
                game.Play();
            }

            if (game.Won)
            {
                Interlocked.Increment(ref won);
            }
            if (game.Tableau.DiscardPiles.Count > 0)
            {
                Interlocked.Increment(ref discards);
            }
            Interlocked.Increment(ref played);
            Interlocked.Add(ref moves, game.Tableau.Moves.Count);
            if (game.Won)
            {
                Interlocked.Add(ref movesWon, game.Tableau.Moves.Count);
            }
            else
            {
                Interlocked.Add(ref movesLost, game.Tableau.Moves.Count);
            }
            Results[game.Seed - Seed] = game.Won;
            Instances[game.Seed - Seed] = game.Instance;
        }

        private void SetCoefficients()
        {
            int suits = Variation.NumberOfSuits;
            double[] coefficients = null;
            if (suits == 1)
            {
                coefficients = Game.OneSuitCoefficients;
            }
            else if (suits == 2)
            {
                coefficients = Game.TwoSuitCoefficients;
            }
            else if (suits == 4)
            {
                coefficients = Game.FourSuitCoefficients;
            }
            InitialCoefficients = new List<double>(coefficients).ToArray();
            Coefficients = new List<double>(coefficients).ToArray();
        }

        public void Play()
        {
            SetCoefficients();

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

            double wonPercent = 100.0 * Won / Played;
            double discardPercent = 100.0 * Discards / Played;
            double averageMoves = (double)Moves / Played;
            double wonMoves = (double)MovesWon / Math.Max(1, Won);
            double lostMoves = (double)MovesLost / Math.Max(1, Played - Won);
            Console.WriteLine("games played: {0}, games won: {1:G5}%, games with discards: {2:G5}%", Played, wonPercent, discardPercent);
            Console.WriteLine("average moves: {0:G5} (won: {1:G5}, lost: {2:G5})", averageMoves, wonMoves, lostMoves);

            if (Debugger.IsAttached)
            {
                Console.ReadKey();
            }
        }

        public void EvaluateCoefficient()
        {
            SetCoefficients();

            if (Coefficient == -1)
            {
                for (int i = 0; i < Coefficients.Length; i++)
                {
                    EvaluateCoefficient(i);
                    PrintCoefficients(Coefficients);
                    Console.WriteLine("");
                }
            }
            else
            {
                EvaluateCoefficient(Coefficient);
            }
        }

        public void EvaluateCoefficient(int coefficient)
        {
            int iterations = 21;
            double factor = 1.5;
            double initialValue = InitialCoefficients[coefficient];
            double minValue = initialValue / factor;
            double maxValue = initialValue * factor;
            double multipler = Math.Exp((Math.Log(Math.Abs(maxValue)) - Math.Log(Math.Abs(minValue))) / (iterations - 1));
            double value = minValue;
            double maxPercentage = 0;
            double bestValue = 0;
            for (int i = 0; i < iterations; i++)
            {
                Coefficients[coefficient] = value;
                PlayOneSet();
                double percentage = 100.0 * won / played;
                if (percentage > maxPercentage)
                {
                    maxPercentage = percentage;
                    bestValue = value;
                }
                Console.WriteLine("Coefficient[{0}] = {1,-12:G6} {2,-8:G5}", coefficient, value, percentage);
                value *= multipler;
            }
            Coefficients[coefficient] = bestValue;
        }

        public void Minimize()
        {
            SetCoefficients();

            Console.WriteLine("Starting minimization...");
            SimplexConstant[] constants = new SimplexConstant[Coefficients.Length];
            for (int i = 0; i < Coefficients.Length; i++)
            {
                constants[i] = new SimplexConstant(Coefficients[i], Math.Abs(Coefficients[i]) / 2);
            }
            double tolerance = 1e-6;
            int maxEvals = 1000;
            ObjectiveFunctionDelegate objFunction = new ObjectiveFunctionDelegate(SpiderObjectiveFunction);
            RegressionResult result = NelderMeadSimplex.Regress(constants, tolerance, maxEvals, objFunction);
            Coefficients = result.Constants;
            PrintCoefficients(Coefficients);
        }

        private double SpiderObjectiveFunction(double[] constants)
        {
            // Evaluate wins.
            Coefficients = constants;
            PlayOneSet();
            double percentage = 100.0 * won / played;
            PrintCoefficients(constants);
            Console.WriteLine("        percentage: {0:F6}", percentage);
            return (100 - percentage) * (100 - percentage);
        }

        private void PrintCoefficients(double[] coefficients)
        {
            Console.Write("Coefficients = new double[] {");
            for (int i = 0; i < coefficients.Length; i++)
            {
                if (i == Game.Group0 || i == Game.Group1)
                {
                    Console.WriteLine();
                    Console.Write("    /* {0} */", i);
                }
                Console.Write(" {0:G10},", coefficients[i]);
            }
            Console.WriteLine();
            Console.WriteLine("};");
        }

#if false
        private delegate double f_delegate(double[] par);

        private class lm_data_type
        {
            public f_delegate f;
        }

        private double my_fit_function(double[] p)
        {
            // Evaluate wins.
            Coefficients = p;
            PlayOneSet();
            double percentage = 100.0 * won / played;
            return percentage;
        }

        public void lm_evaluate(double[] par, int m_dat, double[] fvec,
                         object data, ref int info)
        /*
         *      par is an input array. At the end of the minimization, it contains
         *        the approximate solution vector.
         *
         *      m_dat is a positive integer input variable set to the number
         *        of functions.
         *
         *      fvec is an output array of length m_dat which contains the function
         *        values the square sum of which ought to be minimized.
         *
         *      data is a read-only pointer to lm_data_type
         *
         *      info is an integer output variable. If set to a negative value, the
         *        minimization procedure will stop.
         */
        {
            lm_data_type mydata;
            mydata = (lm_data_type)data;
            double percentage = mydata.f(par);
            for (int i = 0; i < m_dat; i++)
            {
                fvec[i] = 100 - percentage;
            }
        }

        private void lm_print(int n_par, double[] par, int m_dat, double[] fvec,
                              object data, int iflag, int iter, int nfev)
        /*
         *       data  : for soft control of printout behaviour, add control
         *                 variables to the data struct
         *       iflag : 0 (init) 1 (outer loop) 2(inner loop) -1(terminated)
         *       iter  : outer loop counter
         *       nfev  : number of calls to evaluate
         */
        {
            lm_data_type mydata;
            mydata = (lm_data_type)data;

            if (iflag == 2)
            {
                Console.Write("trying step in gradient direction\n");
            }
            else if (iflag == 1)
            {
                Console.Write("determining gradient (iteration {0})\n", iter);
            }
            else if (iflag == 0)
            {
                Console.Write("starting minimization\n");
            }
            else if (iflag == -1)
            {
                Console.Write("terminated after {0} evaluations\n", nfev);
            }

            Console.Write("  par: ");
            for (int i = 0; i < n_par; ++i)
                Console.Write(" {0,12:G6}", par[i]);
            Console.Write(" => {0,12:G6}\n", 100 - fvec[0]);

            if (iflag == -1)
            {
                Coefficients = par;
            }
        }

        public void Minimize()
        {
            int n_p = Coefficients.Length;
            int m_dat = n_p;

            // data and pameter arrays:

            double[] p = new double[Coefficients.Length];
            Coefficients.CopyTo(p, 0);

            // auxiliary settings:

            lmmin.lm_control_type control = new lmmin.lm_control_type();
            lmmin.lm_initialize_control(control);
            control.epsilon = 0.001;

            lm_data_type data = new lm_data_type();
            data.f = my_fit_function;

            // perform the fit:

            lmmin.lm_minimize(m_dat, n_p, p, lm_evaluate, lm_print,
                data, control);

            // print results:

            Console.Write("status: {0} after {1} evaluations\n",
                lmmin.lm_shortmsg[control.info], control.nfev);

            Console.Write("Coefficients = new double[] {");
            for (int i = 0; i < Coefficients.Length; i++)
            {
                if (i == 0 || i == 5)
                {
                    Console.WriteLine();
                    Console.Write("    /* {0} */", i);
                }
                Console.Write(" {0:G10},", Coefficients[i]);
            }
            Console.WriteLine();
            Console.WriteLine("};");
        }
#endif

        public void Compare()
        {
            SetCoefficients();

            while (true)
            {
                ComplexMoves = false;
                PlayOneSet();
                int won1 = Won;
                bool[] results1 = Results;
                int[] instances1 = Instances;
                ComplexMoves = true;
                PlayOneSet();
                int won2 = Won;
                bool[] results2 = Results;
                int[] instances2 = Instances;
                if (won1 != won2)
                {
                    for (int i = 0; i < results1.Length; i++)
                    {
                        if (results1[i] != results2[i])
                        {
                            Console.WriteLine("Game: {0}, Seed: {1}, Instance: {2}/{3}, Won: {4}/{5}",
                                i, Seed + i, instances1[i], instances2[i], results1[i] ? 1 : 0, results2[i] ? 1 : 0);
                        }
                    }
                    break;
                }
                break;
            }
        }

        public void PlayOneSet()
        {
            played = 0;
            won = 0;
            discards = 0;
            moves = 0;
            movesWon = 0;
            movesLost = 0;
            instance = 0;
            Results = new bool[Games];
            Instances = new int[Games];
            currentSeed = Seed;
            int threads = Threads;
            if (threads == -1)
            {
                threads = Environment.ProcessorCount;
            }
            if (Debugger.IsAttached || Interactive)
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
