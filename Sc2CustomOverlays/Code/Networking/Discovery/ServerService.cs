using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using Sc2CustomOverlays.Code.Networking.Discovery.Shared;
using System.Windows;

namespace Sc2CustomOverlays.Code.Networking.Discovery
{
    static class ServerService
    {
        // Gives the current status of the ServerService
        public static bool IsDiscoverable
        {
            get { return discoverable; }
        }

        private static bool discoverable = false;
        private static ServerInfo registeredServer = null;
        private static Thread discoverListener = null;
        private static UdpClient server = null;

        private static Dictionary<IPAddress, DateTime> requestHistory = new Dictionary<IPAddress, DateTime>();

        public static void StartService(ServerInfo regServer)
        {
            registeredServer = regServer;
            
            // Clean up threads and sockets if needed
            if (discoverListener != null)
                discoverListener.Abort();

            if (server != null)
                server.Close();

            // Share a socket with both receiving requests and sending responses
            server = new UdpClient(DiscoveryConstants.ServerPort);

            // Single thread to listen for requests and send out responses
            discoverListener = new Thread(CheckDiscoverRequest);
            discoverListener.Start();

            discoverable = true;
        }


        public static void StopService()
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
            discoverable = false;

        }

        public static void NotifyClosing()
        {
            StopService();
        }

        private static void CheckDiscoverRequest()
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

        private static void SendResponse(IPEndPoint recipient)
        {
            // Ignore this request if they requested in the last 5 seconds.
            if (requestHistory.ContainsKey(recipient.Address) && requestHistory[recipient.Address].AddSeconds(5) > DateTime.Now)
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
            requestHistory[recipient.Address] = DateTime.Now;
        }



    }
}
