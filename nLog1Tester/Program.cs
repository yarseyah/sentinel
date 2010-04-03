using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nLog1Tester
{
    using System;
    using System.Threading;

    class Program
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        private static readonly Random random = new Random();

        static void Main()
        {
            for (int i = 0; i < 1000; i++)
            {
                log.Trace("Trace message {0}", i);
                Thread.Sleep(random.Next(100));
                log.Debug("Debug message {0}", i);
                Thread.Sleep(random.Next(100));
                log.Warn("Warn message {0}", i);
                Thread.Sleep(random.Next(100));
                log.Info("Info message {0}", i);
                Thread.Sleep(random.Next(100));
                log.Error("Error message {0}", i);
                Thread.Sleep(random.Next(100));
                log.Fatal("Fatal message {0}", i);
                Thread.Sleep(random.Next(100));
            }
        }
    }
}
