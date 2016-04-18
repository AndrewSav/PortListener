// This is a very basic TCP port listener that allows you to listen on a port range
// If you run this program outside of firewall and run a port scanner inside a firewall
// pointing to the ip address where this program runs, the port scanner will be able you
// to tell which exactly ports are open on the firewall
// This code will run on Windows, but most importantly also on linux.
// DigitalOcean.com has all ports for their VMs open by default. So spin a new VM,
// copy pln.cs in your (root) home folder and then run:
// sudo apt-get update
// sudo apt-get install mono-complete -y
// mcs pln.cs
// ulimit -n 66000
// ./pln.exe 1 65535
// Now you can use the VM ip address to determine open ports on your firewall
// Note that this is a dev utility, and is aimed to be minimal - no input validation,
// no error handling. In case of a error stack trace is dumpled to console

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace PortListener
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Usage: pln.exe startPort [endPort]");
                Listen(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }
        }

        private static void Listen(string[] args)
        {
            int startPort = int.Parse(args[0]);
            int endPort = int.Parse(args[1]);
            Console.WriteLine("Started binding...");

            Stopwatch swStart = new Stopwatch();
            Stopwatch swLoop = new Stopwatch();
            Stopwatch swListen = new Stopwatch();

            for (int i = startPort; i <= endPort; i++)
            {
                swLoop.Start();
                if (i % 100 == 0 && Environment.OSVersion.Platform != PlatformID.Unix)
                {
                    Console.WriteLine("Binding port " + i);
                    Console.WriteLine($"{swStart.Elapsed};{swListen.Elapsed};{swLoop.Elapsed}");
                }
                TcpListener listener = new TcpListener(IPAddress.Any, i);
                try
                {
                    swStart.Start();
                    listener.Start();
                    swStart.Stop();
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
                    {
                        Console.WriteLine("Port " + i + " already in use");
                        continue;
                    }
                    if (ex.SocketErrorCode == SocketError.AccessDenied)
                    {
                        Console.WriteLine("Access denied to port " + i);
                        continue;
                    }
                    throw;
                }
                swListen.Start();
                listener.BeginAcceptSocket(DoAcceptSocketCallback, listener);
                swListen.Stop();
                swLoop.Stop();
            }
            Console.WriteLine("Finished binding. Ctrl-C to stop.");
            Console.ReadLine();

        }
        private static void DoAcceptSocketCallback(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;
            listener.EndAcceptSocket(ar).Close();
            Console.WriteLine("Connection on port " + ((IPEndPoint)listener.LocalEndpoint).Port);
            listener.BeginAcceptSocket(DoAcceptSocketCallback, listener);
        }
    }
}