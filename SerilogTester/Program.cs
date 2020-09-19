namespace SerilogTester
{
    using System;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    using Bogus.Extensions;

    using Serilog;
    using Serilog.Sinks.Udp.TextFormatters;

    public static class Program
    {
        private static Random random = new Random(1234);

        private static void Main()
        {
            var logger = new LoggerConfiguration()
                .WriteTo.Udp("localhost", 9999, AddressFamily.InterNetwork, new Log4jTextFormatter())
                .WriteTo.Console()
                .MinimumLevel.Verbose()
                .CreateLogger();

            ////var logger = new LoggerConfiguration()
            ////    .WriteTo.Udp("localhost", 9998, AddressFamily.InterNetwork, new Log4netTextFormatter())
            ////    .WriteTo.Console()
            ////    .MinimumLevel.Verbose()
            ////    .CreateLogger();

            var sourceGenerator = new SourceGenerator();
            var messageGenerator = new MessageGenerator();
            var logLevelGenerator = new LogLevelGenerator();

            while (true)
            {
                var source = sourceGenerator.Generate();
                var message = messageGenerator.Generate();

                switch (logLevelGenerator.Generate())
                {
                    case 1:
                        logger.Information("Source: {@source}, Message: {@message}", source, message);
                        break;
                    case 2:
                        logger.Warning("{@source}, {@message}", source, message);
                        break;
                    case 3:
                        logger.Error("Something went wrong: {@source}, {@message}", source, message);
                        break;
                    case 4:
                        var exception = new MissingFieldException(message.Value);
                        logger.Fatal(exception, "Fatal Error!!! {@message}", message);
                        break;
                    case 5:
                    case 6:
                        logger.Debug("Simple debugging output {@source}, {@message}", source, message);
                        break;
                    default:
                        logger.Verbose("{@source} = {@message}", source.Name, message.Value);
                        break;
                }

                Thread.Sleep(1000);
            }
        }
    }
}