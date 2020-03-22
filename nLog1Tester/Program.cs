namespace NLog1Tester
{
    using System;
    using System.Threading;

    using NLog;

    public static class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly Random Random = new Random();

        public static void Main()
        {
            for (int i = 0; i < 1000; i++)
            {
                Log.Trace("Trace message {0}", i);
                Thread.Sleep(Random.Next(100));
                Log.Debug("Debug message {0}", i);
                Thread.Sleep(Random.Next(100));
                Log.Warn("Warn message {0}", i);
                Thread.Sleep(Random.Next(100));
                Log.Info("Info message {0}", i);
                Thread.Sleep(Random.Next(100));
                Log.Error("Error message {0}", i);
                Thread.Sleep(Random.Next(100));
                Log.Fatal("Fatal message {0}", i);
                Thread.Sleep(Random.Next(100));
            }
        }
    }
}