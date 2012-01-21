using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sc2CustomOverlays.Code.Networking
{
    namespace StatusConstants
    {
        public class BaseStatus
        {
            private string status;
            public BaseStatus(string statusValue) { status = statusValue; }
            public override string  ToString() { return status; }
        }

        public sealed class ConnectionStatus : BaseStatus
        {
            public static ConnectionStatus Connecting = new ConnectionStatus("Connecting");
            public static ConnectionStatus Authenticating = new ConnectionStatus("Authenticating");
            public static ConnectionStatus Connected = new ConnectionStatus("Connected");
            public static ConnectionStatus NotConnected = new ConnectionStatus("Not Connected");

            private ConnectionStatus(string status) : base(status) { }
        }

        public sealed class ListenStatus : BaseStatus
        {

            public static ListenStatus Listening = new ListenStatus("Listening");
            public static ListenStatus Authenticating = new ListenStatus("Authenticating");
            public static ListenStatus Connected = new ListenStatus("Connected");
            public static ListenStatus NotListening = new ListenStatus("Not Listening");

            private ListenStatus(string status) : base(status) { }
        }

    }
}
