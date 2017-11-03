using System;
using System.Net;
using Itg.ZabbixAgent.ValueProviders;
using Mono.Options;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Itg.ZabbixAgent.SampleAgent
{
    internal class Program
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

        private static void Main(string[] args)
        {
            var port = 10050;
            var argumentParser = new OptionSet
            {
                {"p=|port=", (int v) => port = v }
            };
            argumentParser.Parse(args);

            InitializeNLog();

            var storedValueProvider = new StoredValueProvider();
            storedValueProvider.SetValue("test", 42);

            var delegateValueProvider = new DelegateValueProvider();
            delegateValueProvider.AddItem("echo", a => a);

            using (var server = new PassiveCheckServer(new IPEndPoint(0, port), storedValueProvider, delegateValueProvider))
            {
                server.Start();
                Console.ReadLine();
            }
        }
    }
}
