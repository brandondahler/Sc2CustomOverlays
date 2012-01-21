using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using Sc2CustomOverlays.Code;
using Sc2CustomOverlays.Code.OverlayVariables;
using System.Net;
using Sc2CustomOverlays.Code.Networking.Control;
using Sc2CustomOverlays.Code.Networking.StatusConstants;
using Sc2CustomOverlays.Code.Networking.Encryption;
using System.Windows;

namespace Sc2CustomOverlays.Code.Networking.Control
{
    class ControlServerService : ControlService
    {

        #region Instance
            private static ControlServerService _instance = new ControlServerService();
            public static ControlServerService Instance { get { return _instance; } }
        #endregion

        // Public Notifiable Properties
        #region ListeningStatus
            private ListenStatus _listeningStatus = null;
            public ListenStatus ListeningStatus
            {
                get { return _listeningStatus; }
                set
                {
                    _listeningStatus = value;
                    RaisePropertyChanged("ListeningStatus");
                }
            }
        #endregion

        // Protected Properties
        #region Server
            private EncryptedTcpListener _server = null;
            protected EncryptedTcpListener Server
            {
                get { return _server; }
                set
                {
                    if (_server != null)
                    {
                        _server.Stop();
                        ListeningStatus = ListenStatus.NotListening;
                    }

                    _server = value;
                }
            }
        #endregion

        // Private Data Types
        private struct SocketThreadInfo
        {
            public ushort port;
            public string password;
        }

        // This is a singleton-style class, use the Instance static property to get the instance.
        private ControlServerService()
        {
            ListeningStatus = ListenStatus.NotListening;
        }

        #region Start/Stop Service
            public void StartService(ushort port, string password)
            {
                base.StartService();

                SocketThread = new Thread(ListenerThread);
                SocketThread.Start(new SocketThreadInfo() { port = port, password = password });
            }

            public override void StopService()
            {
                base.StopService();
                Server = null;
            }
        #endregion

        #region Listener Thread
            private void ListenerThread(object threadParams)
            {
                SocketThreadInfo threadInfo = (SocketThreadInfo) threadParams;

                Server = new EncryptedTcpListener(IPAddress.Any, threadInfo.port);
                Server.Start();
            
                while (true)
                {
                    ListeningStatus = ListenStatus.Listening;    

                    Connection = Server.AcceptEncryptedTcpClient();
                    ConnectedStatus = ConnectionStatus.Connecting;
                
                    ListeningStatus = ListenStatus.Authenticating;
                    ConnectedStatus = ConnectionStatus.Authenticating;
                    EncryptedStream = Connection.GetEncryptedStream(threadInfo.password);

                    ListeningStatus = ListenStatus.Connected;
                    ConnectedStatus = ConnectionStatus.Connected;

                    try
                    {
                        ProcessData();
                    } catch (Exception ex) {
                    }

                    Connection = null;
                }
            }
        #endregion


    }
}
