using System.Net.Sockets;

namespace PortListener
{
    class ListenerState
    {
        public TcpListener Listener { get; set; }
        public Config Config { get; set; }
    }
}
