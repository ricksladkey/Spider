using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    class Program
    {
        static void Main(string[] args)
        {
            bool minimize = false;
            bool evaluate = false;
            bool compare = false;
            Player player = new Player();
            int i = 0;
            while (i < args.Length)
            {
                string arg = args[i];
                if (arg == "--threads")
                {
                    player.Threads = int.Parse(args[i + 1]);
                    i += 2;
                    continue;
                }
                if (arg == "--games")
                {
                    player.Games = int.Parse(args[i + 1]);
                    i += 2;
                    continue;
                }
                if (arg == "--seed")
                {
                    player.Seed = int.Parse(args[i + 1]);
                    i += 2;
                    continue;
                }
                if (arg == "--suits")
                {
                    player.Suits = int.Parse(args[i + 1]);
                    i += 2;
                    continue;
                }
                if (arg == "--coefficient")
                {
                    player.Coefficient = int.Parse(args[i + 1]);
                    i += 2;
                    evaluate = true;
                    continue;
                }
                if (arg == "--minimize")
                {
                    i += 1;
                    minimize = true;
                    continue;
                }
                if (arg == "--trace")
                {
                    player.TraceStartFinish = true;
                    player.TraceDeals = true;
                    player.TraceMoves = true;
                    i++;
                    continue;
                }
                if (arg == "--complex_moves")
                {
                    player.ComplexMoves = true;
                    i++;
                    continue;
                }
                if (arg == "--diagnostics")
                {
                    player.Diagnostics = true;
                    i++;
                    continue;
                }
                if (arg == "--show_results")
                {
                    player.ShowResults = true;
                    i++;
                    continue;
                }
                if (arg == "--record_complex")
                {
                    player.RecordComplex = true;
                    i++;
                    continue;
                }
                if (arg == "--compare")
                {
                    compare = true;
                    i++;
                    continue;
                }
                if (arg.Substring(0, 2) == "--")
                {
                    Console.WriteLine("invalid argument: " + arg);
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                    return;
                }
                break;
            }
            if (i != args.Length)
            {
                Console.WriteLine("extra argument: " + args[i]);
                return;
            }
            if (evaluate)
            {
                player.EvaluateCoefficient();
            }
            else if (minimize)
            {
                player.Minimize();
            }
            else if (compare)
            {
                player.Compare();
            }
            else
            {
                player.Play();
            }
        }
    }
}
