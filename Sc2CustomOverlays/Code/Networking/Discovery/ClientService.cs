using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sc2CustomOverlays.Code.Networking.Discovery.Shared;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Windows;

namespace Sc2CustomOverlays.Code.Networking.Discovery
{
    static class ClientService
    {
        public delegate void ServerDiscoveredHandler(IPAddress serverIp, ServerInfo server);
        private static event ServerDiscoveredHandler ServerDiscovered;
        private static uint handlerCount = 0;

        private static Thread discoveryRequestThread = null;
        private static Thread discoveryResponseThread = null;

        private static UdpClient client = null;

        public static void RegisterDiscoveryHandler(ServerDiscoveredHandler handler)
        {
            // Add handler and increment count
            ServerDiscovered += handler;
            ++handlerCount;

            // If this is the first handler, start discovery
            if (handlerCount == 1)
                StartDiscovery();
        }

        public static void UnregisterDiscoveryHandler(ServerDiscoveredHandler handler)
        {
            // Decrement count
            --handlerCount;

            // Stop discovery if this is the last handler
            if (handlerCount == 0)
                StopDiscovery();

            // Remove handler
            ServerDiscovered -= handler;
        }

        public static void NotifyClosing()
        {
            StopDiscovery();
        }

        private static void StartDiscovery()
        {
            // Cleanup any dirty threads or sockets
            if (discoveryRequestThread != null)
                discoveryRequestThread.Abort();

            if (discoveryResponseThread != null)
                discoveryResponseThread.Abort();

            if (client != null)
                client.Close();
            
            // Create udp socket to receive on, sending done on separate broadcast socket.
            client = new UdpClient(new IPEndPoint(IPAddress.Any, DiscoveryConstants.ClientPort));

            // Start new threads, one each for sending requests and receiving responses
            discoveryRequestThread = new Thread(DiscoverServersRequest);
            discoveryResponseThread = new Thread(DiscoverServerResponse);

            discoveryRequestThread.Start();
            discoveryResponseThread.Start();
        }

        private static void DiscoverServersRequest()
        {
            IPAddress[] localIps = Dns.GetHostAddresses(Dns.GetHostName());
            List<IPAddress> localV4Ips = new List<IPAddress>();

            // Only use IPv4 addresses for now
            foreach (IPAddress localIp in localIps)
            {
                if (localIp.AddressFamily == AddressFamily.InterNetwork)
                    localV4Ips.Add(localIp);
            }

            // Prepare the request ahead of time
            byte[] request = new byte[1];
            request[0] = (byte)DiscoveryConstants.Requests.Request;

            while (true)
            {
                // For each interface's ip, send a broadcasted request
                foreach (IPAddress localIp in localV4Ips)
                {
                    // Create socket to broadcast on
                    UdpClient broadcastClient = new UdpClient(new IPEndPoint(localIp, DiscoveryConstants.ClientPort));
                    broadcastClient.EnableBroadcast = true;

                    // Broadcast data and close
                    broadcastClient.Send(request, 1, new IPEndPoint(IPAddress.Broadcast, DiscoveryConstants.ServerPort));
                    broadcastClient.Close();
                }

                // Don't flood the networks with requests
                Thread.Sleep(2500);
            }
        }

        private static void DiscoverServerResponse()
        {
            while (true)
            {
                try
                {
                    // Receive responses from servers
                    IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, DiscoveryConstants.ServerPort);
                    byte[] response = client.Receive(ref serverEndPoint);

                    // Check that it is a response
                    if (response.Length > 1 && (DiscoveryConstants.Responses)response[0] == DiscoveryConstants.Responses.Response)
                    {
                        // Try to parse out a ServerInfo
                        ServerInfo receivedServer = new ServerInfo();
                        int responseIndex = receivedServer.FromBytes(response, 1);

                        // If the ServerInfo parsed out, raise ServerDiscovered event.
                        if (responseIndex > 0)
                            ServerDiscovered(serverEndPoint.Address, receivedServer);
                    }

                } catch (SocketException) {
                    // Receive will throw an exception when the socket is closed, 
                    //  aborting the thread does not unblock and stop the receive.
                    return;
                }

            }
        }

        private static void StopDiscovery()
        {
            // Cleanup threads
            if (discoveryRequestThread != null)
            {
                discoveryRequestThread.Abort();
                discoveryRequestThread = null;
            }

            if (discoveryResponseThread != null)
            {
                discoveryResponseThread.Abort();
                discoveryResponseThread = null;
            }

            // Close the socket to unblock receive thread and allow it to abort
            if (client != null)
            {
                client.Close();
                client = null;
            }
        }

        
    }
}
