namespace Log4NetTester
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using log4net;

    public static class Program
    {
        private static readonly ILog Log = LogManager.GetLogger("Log4NetTester");

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
            var i = 0;

            var smallestSleep = 100;
            var biggestSleep = 200;

            while (i < 100000)
            {
                // Randomly generate a message:
                var text = RandomMessage(i++);

                try
                {
                    // Randomly assign a message
                    LogMessage(text);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                Thread.Sleep(Random.Next(smallestSleep, biggestSleep));
            }
        }

        private static void LogMessage(string text)
        {
            var randomType = Random.Next(0, 5);

            switch (randomType)
            {
                case 0:
                    var embeddedException = new NotSupportedException();
                    var keyNotFoundException = new KeyNotFoundException("Some wrapped message", embeddedException);
                    Log.Error(text, keyNotFoundException);
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
                default:
                    Log.Debug(text);
                    break;
            }
        }

        private static string RandomMessage(int i)
        {
#if !TESTING_MESSAGE_THROUGHPUT
            var randomNumber = Random.Next(0, 5);
#else
            int randomNumber = Int32.MaxValue;
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