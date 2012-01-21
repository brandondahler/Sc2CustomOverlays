using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Sc2CustomOverlays.Code.Networking.Encryption
{
    public class EncryptedTcpClient : TcpClient
    {
        public EncryptedTcpClient() : base() { }
        public EncryptedTcpClient(AddressFamily family) : base(family) { }
        public EncryptedTcpClient(IPEndPoint localEP) : base(localEP) { }
        public EncryptedTcpClient(string hostname, int port) : base(hostname, port) { }


        public EncryptedNetworkStream GetEncryptedStream(string password)
        {
            return new EncryptedNetworkStream(Client, password);
        }

    }
}
