using System;
using System.Net;
using Mono.Options;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Itg.ZabbixAgentLib.SampleAgent
{
    class Program
    {
        private static void InitializeNLog()
        {
            var config = new LoggingConfiguration();

            var consoleTarget = new ColoredConsoleTarget();
            var consoleTargetRule = new LoggingRule("*", LogLevel.Trace, consoleTarget);
            config.LoggingRules.Add(consoleTargetRule);

            LogManager.Configuration = config;
            LogManager.ReconfigExistingLoggers();
        }

        static void Main(string[] args)
        {
            int port = 10050;
            var argumentParser = new OptionSet
            {
                {"p=|port=", (int v) => port = v },
            };
            argumentParser.Parse(args);

            InitializeNLog();

            using (var server = new PassiveCheckServer(new IPEndPoint(0, port)))
            {
                server.AddItem("test", () => 42);
                server.AddItem("echo", a => a);

                server.Start();
                Console.ReadLine();
            }
        }
    }
}
