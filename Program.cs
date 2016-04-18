using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Mono.Options;

namespace PortListener
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string exe = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
                Console.WriteLine(exe + " - Port Listener");
                Config config;
                if (!ParseArgs(args, out config))
                {
                    return;
                }
                Listen(config);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }
        }

        private static bool ParseArgs(string[] args, out Config config)
        {
            config = null;
            Config result = new Config();
            StringBuilder parsingErrors = new StringBuilder();
            OptionSet optionSet = new OptionSet
            {
                {"b:", "display binding progress every VALUE ports. Default 100.", s => ParseDisplayBindingProgress(s, ref result, parsingErrors)},
                {"c", "display incoming connections", s => result.DisplayIncomingConnections = s != null},
                {"s", "suppress binding errors", s => result.SuppressBindingErrors = s != null},
                {"t", "display timestamp", s => result.DisplayTimestamp = s != null}
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

        private static void PrintUsage(OptionSet optionSet)
        {
            string exe = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
            Console.WriteLine("Usage: " + exe + " startPort [endPort] [/b=[VALUE]] [/c] [/s] [/t]");
            optionSet.WriteOptionDescriptions(Console.Out);
            Console.WriteLine();
            Console.WriteLine("This is a very basic TCP port listener that allows you to listen on a port range");
            Console.WriteLine("If you run this program outside of firewall and run a port scanner inside a firewall");
            Console.WriteLine("pointing to the ip address where this program runs, the port scanner will be able you");
            Console.WriteLine("to tell which exactly ports are open on the firewall.");
            Console.WriteLine();
            Console.WriteLine("This code will run on Windows, but most importantly also on linux.");
            Console.WriteLine("DigitalOcean.com has all ports for their VMs open by default. So spin a new VM,");
            Console.WriteLine("copy pln.cs in your (root) home folder and then run:");
            Console.WriteLine("# apt-get update");
            Console.WriteLine("# apt-get install mono-mcs -y");
            Console.WriteLine("# mcs Program.cs Config.cs ListenerState.cs Options.cs -out:pln.exe");
            Console.WriteLine("# ulimit -n 66000");
            Console.WriteLine("# ./pln.exe 1 65535");
            Console.WriteLine();
            Console.WriteLine("Now you can use the VM ip address to determine open ports on your firewall");
            Console.WriteLine("Note that this is a dev utility, and is aimed to be minimal,");
            Console.WriteLine("no error handling. In case of a error stack trace is dumpled to console");
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


        private static void Listen(Config config)
        {

            config.WriteProgress("Started binding...");

            for (uint i = config.PortStart; i <= config.PortEnd; i++)
            {
                if (config.DisplayBindingProgress)
                {
                    if (i % config.BindingProgressInterval == 0)
                    {
                        config.WriteProgress("Binding port " + i);
                    }
                }
                TcpListener listener = new TcpListener(IPAddress.Any, (int)i);
                try
                {
                    listener.Start();
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
                    {
                        if (!config.SuppressBindingErrors)
                        {
                            config.WriteProgress("Port " + i + " already in use");
                        }
                        continue;
                    }
                    if (ex.SocketErrorCode == SocketError.AccessDenied)
                    {
                        if (!config.SuppressBindingErrors)
                        {
                            config.WriteProgress("Access denied to port " + i);
                        }
                        continue;
                    }
                    throw;
                }
                listener.BeginAcceptSocket(DoAcceptSocketCallback, new ListenerState {Config = config, Listener = listener});
            }
            config.WriteProgress("Finished binding. Ctrl-C to stop.");
            Console.ReadLine();

        }
        private static void DoAcceptSocketCallback(IAsyncResult ar)
        {
            ListenerState listenerState = (ListenerState)ar.AsyncState;
            listenerState.Listener.EndAcceptSocket(ar).Close();
            if (listenerState.Config.DisplayIncomingConnections)
            {
                listenerState.Config.WriteProgress("Connection on port " + ((IPEndPoint) listenerState.Listener.LocalEndpoint).Port);
            }
            listenerState.Listener.BeginAcceptSocket(DoAcceptSocketCallback, listenerState);
        }
    }
}