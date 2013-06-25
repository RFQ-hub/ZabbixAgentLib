using System;
using System.Collections.Generic;
using System.Text;
using Mono.Options;

namespace SampleAgent
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 10051;
            string activeServer = null;
            string hostName = "tests";
            var argumentParser = new OptionSet
            {
                {"a=|active-server=", v => activeServer = v},
                {"p=|port=", (int v) => port = v },
                {"h=|host=", v => hostName =v }
            };
            argumentParser.Parse(args);


        }
    }
}
