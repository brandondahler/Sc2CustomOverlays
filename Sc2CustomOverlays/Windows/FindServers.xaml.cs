using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Sc2CustomOverlays.Code.Networking.Discovery;
using Sc2CustomOverlays.Code.Networking.Discovery.Shared;
using System.Net;
using System.Reflection;
using System.Collections.ObjectModel;

namespace Sc2CustomOverlays.Windows
{
    public partial class FindServers : Window
    {
        // Event fired when a server has been selected and the user has pushed the Use Selected button.
        public delegate void ServerSelectedHandler(DiscoveredServer server);
        public event ServerSelectedHandler ServerSelected;

        // Used to display data in the DataGrid and hold information about discovered servers.
        public struct DiscoveredServer
        {
            public string Name { get; set; }
            public IPAddress Ip { get; set; }
            public ushort Port { get; set; }
        }

        // Holds list of servers that have been discovered in the windows lifetime, bound to the data grid.
        public ObservableCollection<DiscoveredServer> DiscoveredServers { get; set; }

        public FindServers()
        {
            DiscoveredServers = new ObservableCollection<DiscoveredServer>();
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Add handler for ClientService's ServerDiscovered event.  
            //  This will start discovery.
            ClientService.RegisterDiscoveryHandler(ClientService_ServerDiscovered);
        }

        private void ClientService_ServerDiscovered(IPAddress serverIp, ServerInfo server)
        {
            // Fill a new instance of DiscoveredServer
            DiscoveredServer discServer = new DiscoveredServer();
            discServer.Name = server.Name;
            discServer.Ip = serverIp;
            discServer.Port = server.Port;

            // Only add new servers, DiscoveredServer must be a struct for this contain to work.
            if (!DiscoveredServers.Contains(discServer))
            {
                // Since DiscoveredServers is bound to the data grid, we must invoke the add on the UI's thread.
                this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(delegate()
                    {
                        DiscoveredServers.Add(discServer);
                    })
                );
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Remove the handler for ClientService's ServerDiscovered. 
            //  This will stop discovery unless there are other handlers listening in.
            ClientService.UnregisterDiscoveryHandler(ClientService_ServerDiscovered);

        }

        private void btnUseSelected_Click(object sender, RoutedEventArgs e)
        {
            object selectedItem = dgFoundServers.SelectedItem;
            if (selectedItem != null)
            {
                ServerSelected( (DiscoveredServer) selectedItem);
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        

    }
}
