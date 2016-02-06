namespace Sentinel.EventLogMonitor
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    using CommandLine;

    using Common.Logging;

    using Newtonsoft.Json;

    public static class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(Program));

        public static void Main(string[] args)
        {
            var options = new CommandLineOptions();
            var isCommandLineValid = ParseCommandLine(args, options);

            if (isCommandLineValid)
            {
                DisplayBanner(options);

                var eventLog = new EventLog { // TODO: pass this on the command line
                                                Log = "Application" };

                eventLog.EntryWritten += NewLogEntryWrittenHandler;
                eventLog.EnableRaisingEvents = true;

                // TODO: keep alive
                if (Debugger.IsAttached)
                {
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                }

            }
        }

        private static void NewLogEntryWrittenHandler(object sender, EntryWrittenEventArgs e)
        {
            Log.TraceFormat("New event log entry written");
            var entry = new EventLogEntry(e.Entry);

            var json = JsonConvert.SerializeObject(entry);

            Console.WriteLine(entry);
            Console.WriteLine("------------------------------");
            Console.WriteLine(json);
            Console.WriteLine("==============================");
        }

        private static void DisplayBanner(CommandLineOptions options)
        {
            if (!options.SuppressBanner)
            {
                Console.WriteLine("Sentinel System Event Monitor");
                Console.WriteLine(Assembly.GetExecutingAssembly().GetName().Version);
                Console.WriteLine();
            }
        }

        private static bool ParseCommandLine(string[] args, CommandLineOptions options)
        {
            try
            {
                var parseResult = Parser.Default.ParseArguments(args, options);

                if (parseResult)
                {
                    Log.Trace("Command line parsing was successful");
                }
                else
                {
                    Log.Warn("Command line parsing was unsuccessful");
                }

                return parseResult;
            }
            catch (Exception e)
            {
                Log.Error("Parsing error caught", e);
                return false;
            }
        }
    }
}
