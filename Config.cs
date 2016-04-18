using System;

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

        public Action<string> WriteProgress
        {
            get
            {
                return DisplayTimestamp
                    ? (Action<string>) (x => Console.WriteLine("[" + DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")+ "] " + x ))
                    : Console.WriteLine;
            }
        }
    }
}
