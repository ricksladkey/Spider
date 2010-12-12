using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Spider.Collections;
using Spider.Engine;
using Spider.GamePlay;

namespace Spider
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = new UTF8Encoding(true);
            bool minimize = false;
            bool evaluate = false;
            bool compare = false;
            Player player = new Player();
            try
            {
                int i = 0;
                while (i < args.Length)
                {
                    string arg = args[i];
                    if (arg == "--threads")
                    {
                        player.NumberOfThreads = int.Parse(args[i + 1]);
                        i += 2;
                        continue;
                    }
                    if (arg == "--games")
                    {
                        player.NumberOfGames = int.Parse(args[i + 1]);
                        i += 2;
                        continue;
                    }
                    if (arg == "--seed")
                    {
                        player.Seed = int.Parse(args[i + 1]);
                        i += 2;
                        continue;
                    }
                    if (arg == "--variation")
                    {
                        player.Variation = Variation.FromAsciiString(args[i + 1]);
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
                    if (arg == "--trace_start_finish")
                    {
                        player.TraceStartFinish = true;
                        i++;
                        continue;
                    }
                    if (arg == "--trace_moves")
                    {
                        player.TraceMoves = true;
                        i++;
                        continue;
                    }
                    if (arg == "--trace_deals")
                    {
                        player.TraceDeals = true;
                        i++;
                        continue;
                    }
                    if (arg == "--trace_search")
                    {
                        player.TraceSearch = true;
                        i++;
                        continue;
                    }
                    if (arg == "--profile")
                    {
                        player.Profile = true;
                        i++;
                        continue;
                    }
                    if (arg == "--algorithm")
                    {
                        player.AlgorithmType = AlgorithmType.Parse(args[i + 1]);
                        i += 2;
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
                    if (arg == "--interactive")
                    {
                        player.Interactive = true;
                        i++;
                        continue;
                    }
                    if (arg == "--show_results")
                    {
                        player.ShowResults = true;
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
            finally
            {
                player.Dispose();
            }
        }
    }
}
