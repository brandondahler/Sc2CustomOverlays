using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Sc2CustomOverlays.Code.Networking.Encryption
{
    public class EncryptedTcpListener : TcpListener
    {
        public EncryptedTcpListener(int port) : base(port) { }
        public EncryptedTcpListener(IPEndPoint localEP) : base(localEP) { }
        public EncryptedTcpListener(IPAddress localaddr, int port) : base(localaddr, port) { }


        public EncryptedTcpClient AcceptEncryptedTcpClient()
        {
            return new EncryptedTcpClient() { Client = AcceptSocket() };
        }

    }
}
