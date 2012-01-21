using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using Sc2CustomOverlays.Code.Networking.Discovery.DiscoveryShared;
using System.Windows;
using System.ComponentModel;

namespace Sc2CustomOverlays.Code.Networking.Discovery
{
    class DiscoveryServerService : INotifyPropertyChanged
    {
        #region Instance
            private static DiscoveryServerService _instance;
            public static DiscoveryServerService Instance
            {
                get 
                {
                    if (_instance == null)
                        _instance = new DiscoveryServerService();

                    return _instance; 
                }
            }

            protected DiscoveryServerService()
            {

            }
        #endregion

        // Notifies if a property has changed
        public event PropertyChangedEventHandler PropertyChanged;

        #region Discoverable
            private bool _discoverable = false;
            public bool Discoverable 
            { 
                get { return _discoverable; } 
                set
                {
                    _discoverable = value;
                    RaisePropertyChanged("Discoverable");
                }
            }
        #endregion
        
        private DiscoveryServerInfo registeredServer = null;
        private Thread discoverListener = null;
        private UdpClient server = null;

        private Dictionary<IPAddress, Queue<DateTime>> requestHistory = new Dictionary<IPAddress, Queue<DateTime>>();
        private DateTime? lastRequestTime = null;

        public void StartService(DiscoveryServerInfo regServer)
        {
            StopService();

            registeredServer = regServer;
            
            // Share a socket with both receiving requests and sending responses
            server = new UdpClient(DiscoveryConstants.ServerPort);

            // Single thread to listen for requests and send out responses
            discoverListener = new Thread(CheckDiscoverRequest);
            discoverListener.Start();

            Discoverable = true;
        }

        private void CheckDiscoverRequest()
        {
            while (true)
            {
                try
                {
                    // Receive requests on any interface
                    IPEndPoint sender = new IPEndPoint(IPAddress.Any, DiscoveryConstants.ClientPort);
                    byte[] discoverRequest = server.Receive(ref sender);

                    if (discoverRequest.Length > 0)
                    {
                        // Send repsonse if this is a valid request
                        if ((DiscoveryConstants.Requests) discoverRequest[0] == DiscoveryConstants.Requests.Request)
                            SendResponse(sender);
                    }
                } catch (SocketException) {
                    // Receive will throw an exception when the socket is closed, 
                    //  aborting the thread does not unblock and stop the receive.
                    return;
                }

            }
        }

        private void SendResponse(IPEndPoint recipient)
        {
            // If the last request was 10 seconds ago, clear all of the history.  Otherwise, just check this specific one.
            if (lastRequestTime.HasValue && lastRequestTime.Value.AddSeconds(10) < DateTime.Now)
                requestHistory.Clear();
            else
                CleanExpiredHistory(recipient.Address, 10);

            // Ignore this request if they still have 10 requests out within the determined amount of time
            if (requestHistory.ContainsKey(recipient.Address) && requestHistory[recipient.Address].Count >= 10)
                return;

            // Get the bytes of the registered server, 
            //  make response after since we need the size of the registeredServer bytes first
            byte[] serverBytes = registeredServer.ToBytes();
            byte[] response = new byte[1 + serverBytes.Length];
            
            // Set the first byte to indicate this is a response and copy in the server bytes after it
            response[0] = (byte) DiscoveryConstants.Responses.Response;
            Array.Copy(serverBytes, 0, response, 1, serverBytes.Length);

            // Send response to the requestor
            server.Send(response, response.Length, recipient);

            // Record that we sent a response
            if (!requestHistory.ContainsKey(recipient.Address))
                requestHistory[recipient.Address] = new Queue<DateTime>();

            requestHistory[recipient.Address].Enqueue(DateTime.Now);
            lastRequestTime = DateTime.Now;
        }

        // Clean expired history, expirySeconds determines how long to check against
        //  address determines which one to check.
        private void CleanExpiredHistory(IPAddress address, int expirySeconds)
        {
            if (address != null)
            {
                if (requestHistory.ContainsKey(address))
                {
                    while (requestHistory[address].Count > 0 && requestHistory[address].Peek().AddSeconds(expirySeconds) < DateTime.Now)
                        requestHistory[address].Dequeue();
                }
            }
        }

        public void StopService()
        {
            if (discoverListener != null)
            {
                discoverListener.Abort();
                discoverListener = null;
            }

            if (server != null)
            {
                server.Close();
                server = null;
            }

            requestHistory.Clear();
            Discoverable = false;

            registeredServer = null;

        }

        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }



    }
}
