using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using Mono.Options;

namespace PortListener
{
    class CommandLine
    {
        public static bool ParseArgs(string[] args, out Config config)
        {
            config = null;
            Config result = new Config();
            StringBuilder parsingErrors = new StringBuilder();
            OptionSet optionSet = new OptionSet
            {
                // ReSharper disable once AccessToModifiedClosure
                {"b:","display binding progress every VALUE ports. Default 100.", s => ParseDisplayBindingProgress(s, ref result, parsingErrors)},
                {"c", "display incoming connections", s => result.DisplayIncomingConnections = s != null},
                {"s", "suppress binding errors", s => result.SuppressBindingErrors = s != null},
                {"t", "display timestamp", s => result.DisplayTimestamp = s != null},
                {"d:", "dump first VALUE bytes of incoming data. Default 255. Only works when /c is specified", s => ParseDataDumpLimit(s, ref result, parsingErrors)},
                {"w=", "incoming data wait time. Default - do not wait. Only works when /d is specified",
                    s => ParseIncomingDataWaitTime(s, ref result, parsingErrors)},
                {"i=", "Only bind on specific interfaces (ip addresses)",
                    s => ParseBindInterfaces(s, ref result, parsingErrors)},
            };
            List<string> remaining = optionSet.Parse(args);
            ParseRemaining(remaining, ref result, parsingErrors);
            if (parsingErrors.Length > 0)
            {
                Console.WriteLine("Error parsing arguments:");
                Console.Write(parsingErrors.ToString());
                PrintUsage(optionSet);
                return false;
            }
            config = result;
            return true;
        }

        private static void ParseBindInterfaces(string s, ref Config result, StringBuilder parsingErrors)
        {
            if (s == null)
            {
                parsingErrors.AppendLine("/i parameter requires a value: /i=127.0.0.1,10.10.10.10");
                return;
            }

            List<IPAddress> list = new List<IPAddress>();
            foreach (string part in s.Split(','))
            {
                IPAddress address;
                if (!IPAddress.TryParse(part, out address))
                {
                    parsingErrors.AppendLine("Invalid value of /i parameter. It should be a comma separated list of ip addresses. You specified: " + s);
                    return;
                }
                list.Add(address);
            }
            result.BoundIpAddress = list.ToArray();
        }

        private static void PrintUsage(OptionSet optionSet)
        {
            string exe = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
            Console.WriteLine("Usage: " + exe + " startPort [endPort] [/b=[VALUE]] [/c] [/s] [/t] [/d=[VALUE]] [/w=VALUE] [i=a.b.c.d,a.b.c.d]");
            optionSet.WriteOptionDescriptions(Console.Out);
        }

        private static void ParseDataDumpLimit(string s, ref Config result, StringBuilder parsingErrors)
        {
            if (s == null)
            {
                result.DumpIncomingData = true;
                result.DataDumpLimit = 255;
                return;
            }
            int u;
            if (!int.TryParse(s, out u) || u < 0)
            {
                parsingErrors.AppendLine("Invalid value of /d parameter. It should be a non-negative number. You specified: " + s);
                return;
            }
            result.DumpIncomingData = true;
            result.DataDumpLimit = u;
        }

        private static void ParseIncomingDataWaitTime(string s, ref Config result, StringBuilder parsingErrors)
        {
            if (s == null)
            {
                parsingErrors.AppendLine("/w parameter requires a value: /w=1000");
                return;
            }
            int u;
            if (!int.TryParse(s, out u) || u < 0)
            {
                parsingErrors.AppendLine("Invalid value of /w parameter. It should be a non-negative number. You specified: " + s);
                return;
            }
            result.IncomingDataWaitTime = u;
        }

        private static void ParseDisplayBindingProgress(string s, ref Config result, StringBuilder parsingErrors)
        {
            if (s == null)
            {
                result.DisplayBindingProgress = true;
                result.BindingProgressInterval = 100;
                return;
            }
            uint u;
            if (!uint.TryParse(s, out u) || u == 0)
            {
                parsingErrors.AppendLine("Invalid value of /b parameter. It should be a number between 1 and 65535. You specified: " + s);
                return;
            }
            result.DisplayBindingProgress = true;
            result.BindingProgressInterval = u;
        }

        private static void ParseRemaining(List<string> remaining, ref Config result, StringBuilder parsingErrors)
        {
            if (remaining.Count > 2)
            {
                parsingErrors.AppendLine("Unknown parameters");
                return;
            }
            if (remaining.Count == 0)
            {
                parsingErrors.AppendLine("startPort/endPort are not specified");
                return;
            }

            uint portStart;
            if (!uint.TryParse(remaining[0], out portStart) || portStart == 0)
            {
                parsingErrors.AppendLine("Invalid startPort. It should be a number between 1 and 65535. You specified: " + remaining[0]);
                return;
            }
            if (remaining.Count == 1)
            {
                result.PortStart = result.PortEnd = portStart;
                return;
            }
            uint portEnd;
            if (!uint.TryParse(remaining[1], out portEnd) || portEnd == 0)
            {
                parsingErrors.AppendLine("Invalid endPort. It should be a number between 1 and 65535. You specified: " + remaining[1]);
                return;
            }
            result.PortStart  = portStart;
            result.PortEnd = portEnd;
        }
    }
}