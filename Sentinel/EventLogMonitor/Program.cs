namespace Sentinel.EventLogMonitor
{
    using System;
    using System.Collections.Generic;
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
            var options = ParseCommandLine(args);

            if (options != null)
            {
                DisplayBanner(options);

                // TODO: pass this on the command line
                var eventLog = new EventLog
                {
                    Log = "Application",
                };

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

        private static CommandLineOptions ParseCommandLine(string[] args)
        {
            try
            {
                CommandLineOptions options = null;
                Parser.Default.ParseArguments<CommandLineOptions>(args)
                    .WithParsed(o => options = o)
                    .WithNotParsed(HandleParseErrors);

                if (options != null)
                {
                    Log.Trace("Command line parsing was successful");
                    return options;
                }

                Log.Warn("Command line parsing was unsuccessful");
            }
            catch (Exception e)
            {
                Log.Error("Parsing error caught", e);
            }

            return null;
        }

        private static void HandleParseErrors(IEnumerable<Error> errors)
        {
            foreach (var error in errors)
            {
                Log.Trace(error);
            }
        }
    }
}