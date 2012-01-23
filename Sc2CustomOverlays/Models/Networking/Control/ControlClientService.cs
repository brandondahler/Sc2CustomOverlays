    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Sc2CustomOverlays.Windows;
using Sc2CustomOverlays.Models.Networking.Control.Commands;
using System.IO;
using System.ComponentModel;
using Sc2CustomOverlays.Models.Networking.StatusConstants;
using Sc2CustomOverlays.Models.Networking.Encryption;
using System.Windows;
using System.Security;

namespace Sc2CustomOverlays.Models.Networking.Control
{
    
    class ControlClientService : ControlService
    {
        #region Instance
            protected static ControlClientService _instance = new ControlClientService();
            public static ControlClientService Instance { get { return _instance; } }
        #endregion

        // Private Data Types
        private struct SocketThreadInfo
        {
            public IPAddress ip;
            public ushort port;
            public string password;
        }


        protected ControlClientService()
        {

        }

        #region Start Service

            public void StartService(IPAddress ip, ushort port, string password)
            {
                base.StartService();

                SocketThread = new Thread(ConnectToRemoteServer);
                SocketThread.Start(new SocketThreadInfo() { ip = ip, port = port, password = password });
            }

        #endregion

        #region Connect To Remote Server
            private void ConnectToRemoteServer(object threadParams)
            {
                SocketThreadInfo threadInfo = (SocketThreadInfo) threadParams;

                Connection = new EncryptedTcpClient();
            
                try
                {
                    ConnectedStatus = ConnectionStatus.Connecting;
                    Connection.Connect(new IPEndPoint(threadInfo.ip, threadInfo.port));

                    ConnectedStatus = ConnectionStatus.Authenticating;
                    EncryptedStream = Connection.GetEncryptedStream(threadInfo.password);

                    ConnectedStatus = ConnectionStatus.Connected;

                    DisplayOpenMenuWithRemoteSettings();

                    ProcessData();

                } catch (Exception ex) {
                }

                Connection = null;
            }
        #endregion

    }
}
