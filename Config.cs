using System;
using System.Net;
using System.Threading;

namespace PortListener
{
    class Config
    {
        public bool DisplayIncomingConnections { get; set; }
        public bool SuppressBindingErrors { get; set; }
        public bool DisplayTimestamp { get; set; }
        public bool DisplayBindingProgress { get; set; }
        public uint BindingProgressInterval { get; set; }
        public uint PortStart { get; set; }
        public uint PortEnd { get; set; }
        public bool DumpIncomingData { get; set; }
        public int IncomingDataWaitTime { get; set; }
        public int DataDumpLimit { get; set; }
        public IPAddress[] BoundIpAddress { get; set; }

        public Config()
        {
            BoundIpAddress = new[] {IPAddress.Any};
        }

        public Action<string> WriteProgress
        {
            get
            {
                return x => Console.WriteLine((DisplayTimestamp ? "[" + DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss") + "] ": "") 
                    + "[" + Thread.CurrentThread.ManagedThreadId + "] " + x);
            }
        }
    }
}
