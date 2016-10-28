using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PortListener
{
    class Connection
    {
        public static void Listen(Config config)
        {
            config.WriteProgress("Started binding...");

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (uint i = config.PortStart; i <= config.PortEnd; i++)
            {
                if (config.DisplayBindingProgress)
                {
                    if (i % config.BindingProgressInterval == 0)
                    {
                        config.WriteProgress("Binding port " + i);
                    }
                }

                foreach (IPAddress ipAddress in config.BoundIpAddress)
                {
                    TcpListener listener = new TcpListener(ipAddress, (int) i);
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
            }
            sw.Stop();
            config.WriteProgress("Finished binding in " + (int)(sw.ElapsedMilliseconds / 1000) + " second(s). Ctrl-C to stop.");
            Console.ReadLine();
        }

        private static void DoAcceptSocketCallback(IAsyncResult ar)
        {
            ListenerState listenerState = (ListenerState)ar.AsyncState;
            Socket socket = listenerState.Listener.EndAcceptSocket(ar);
            listenerState.Listener.BeginAcceptSocket(DoAcceptSocketCallback, listenerState);
            string remote = socket.RemoteEndPoint.ToString();
            string local = socket.LocalEndPoint.ToString();

            if (listenerState.Config.DisplayIncomingConnections)
            {
                listenerState.Config.WriteProgress("Connected "+ remote + "=>" + local);
                if (listenerState.Config.DumpIncomingData)
                {
                    if (listenerState.Config.IncomingDataWaitTime > 0)
                    {
                        listenerState.Config.WriteProgress("Waiting for " + listenerState.Config.IncomingDataWaitTime + "ms for data to arrive");
                        Thread.Sleep(listenerState.Config.IncomingDataWaitTime);
                        listenerState.Config.WriteProgress("Waiting finished");
                    }
                    int available = socket.Available;
                    int received = 0;
                    if (available > 0)
                    {
                        int limit = Math.Min(available, listenerState.Config.DataDumpLimit);
                        byte[] buffer = new byte[limit];
                        received = socket.Receive(buffer, limit, SocketFlags.None);
                        if (received > 0)
                        {
                            Console.WriteLine(Hex.Dump(buffer.Take(received).ToArray()));
                        }
                    }
                    listenerState.Config.WriteProgress("Received: " + received + "; Available: " + available);
                }
                socket.Close();
            }
        }
    }
}