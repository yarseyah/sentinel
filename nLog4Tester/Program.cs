﻿// ReSharper disable once StyleCop.SA1300
// ReSharper disable SA1300
namespace nLog4Tester
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using NLog;

    public static class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly Random Random = new Random();

        private static readonly List<string> Reasons = new List<string>
                                                           {
                                                               "Starting system",
                                                               "Closing system",
                                                               "Data exchange started",
                                                               "Unknown issue encountered",
                                                           };

        private static readonly List<string> Sources = new List<string>
                                                           {
                                                               "Foo",
                                                               "Bar",
                                                               "LongSystemName",
                                                               "Kernel32",
                                                           };

        public static void Main()
        {
            const int smallestSleep = 1000;
            const int biggestSleep = 2000;

            for (var i = 0; ; i++)
            {
                // Randomly generate a message:
                var text = RandomMessage(i);

                // Randomly assign a message
                LogMessage(text);

                Thread.Sleep(Random.Next(smallestSleep, biggestSleep));
            }

            // ReSharper disable once FunctionNeverReturns
        }

        private static void LogMessage(string text)
        {
            var randomType = Random.Next(0, 6);

            var context = $"Context{randomType}";
            MappedDiagnosticsContext.Set("UnitTest", context);

            switch (randomType)
            {
                case 0:
                    Log.Error(text);
                    break;
                case 1:
                    Log.Fatal(text);
                    break;
                case 2:
                    Log.Info(text);
                    break;
                case 3:
                    Log.Warn(text);
                    break;
                case 4:
                    Log.Trace(text);
                    break;
                default:
                    Log.Debug(text);
                    break;
            }

            MappedDiagnosticsContext.Remove("UnitTest");
        }

        private static string RandomMessage(int i)
        {
#if !TESTING_MESSAGE_THROUGHPUT
            var randomNumber = Random.Next(0, 5);
#else
            var randomNumber = Int32.MaxValue;
#endif
            switch (randomNumber)
            {
                case 0:
                    return $"Message {i}";
                case 1:
                    return $"Src:'{RandomSrc()}', Msg:'{RandomReason()} - {i}'";
                case 2:
                    return $"[{RandomSrc()}] {RandomReason()} - {i}";
                case 3:
                    return $"[SimulationTime] {RandomReason()} ({i})";
                case 4:
                    return "UTF-8 test code: \u2019 \u263b \u2660 \u2663 \u2665 \u2666";
                default:
                    return i.ToString();
            }
        }

        private static string RandomReason()
        {
            return Reasons[Random.Next(Reasons.Count)];
        }

        private static string RandomSrc()
        {
            return Sources[Random.Next(Sources.Count)];
        }
    }
}