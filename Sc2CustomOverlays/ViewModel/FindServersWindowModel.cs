// -----------------------------------------------------------------------
// <copyright file="FindServersWindowModel.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Sc2CustomOverlays.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Sc2CustomOverlays.Models.MVVMHelpers;
    using System.Net;
    using Sc2CustomOverlays.Models.Networking.Discovery;
    using System.Collections.ObjectModel;
    using Sc2CustomOverlays.Models.Networking.Discovery.DiscoveryShared;
    using System.Windows.Input;
    using Sc2CustomOverlays.Models.MVVMHelpers.Commands;
    using System.Windows;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    /// 

    public delegate void ServerSelectedHandler(FindServersWindowModel.DiscoveredServer server);
    public delegate void CloseHandler();

    public class FindServersWindowModel : ObservableModel
    {
        // Command
        public ICommand UseSelected { get { return new DelegateCommand(Window_UseSelected); } }
        public ICommand Cancel { get { return new DelegateCommand(Window_Cancel); } }


        // Used to display data in the DataGrid and hold information about discovered servers.
        public struct DiscoveredServer
        {
            public string Name { get; set; }
            public IPAddress Ip { get; set; }
            public ushort Port { get; set; }
        }

        public DiscoveredServer? _selectedServer = null;
        public DiscoveredServer? SelectedServer
        {
            get { return _selectedServer; }
            set
            {
                _selectedServer = value;
                RaisePropertyChanged("SelectedServer");
            }
        }
        
        // Events
        public event ServerSelectedHandler ServerSelected;
        public event CloseHandler Close;

        // Holds list of servers that have been discovered in the windows lifetime, bound to the data grid.
        public ObservableCollection<DiscoveredServer> _discoveredServers = new ObservableCollection<DiscoveredServer>();
        public ObservableCollection<DiscoveredServer> DiscoveredServers { get { return _discoveredServers; } }


        public void Window_Loaded()
        {
            // Add handler for ClientService's ServerDiscovered event.  
            //  This will start discovery.
            DiscoveryClientService.Instance.RegisterDiscoveryHandler(DiscoveryClientService_ServerDiscovered);
        }

        public void Window_Closing()
        {
            DiscoveryClientService.Instance.UnregisterDiscoveryHandler(DiscoveryClientService_ServerDiscovered);
        }

        private void DiscoveryClientService_ServerDiscovered(IPAddress serverIp, DiscoveryServerInfo server)
        {
            Application.Current.Dispatcher.Invoke(new Action(delegate()
                {
                    // Fill a new instance of DiscoveredServer
                    DiscoveredServer discServer = new DiscoveredServer();
                    discServer.Name = server.Name;
                    discServer.Ip = serverIp;
                    discServer.Port = server.Port;

                    // Only add new servers, DiscoveredServer must be a struct for this contain to work.
                    if (!DiscoveredServers.Contains(discServer))
                        DiscoveredServers.Add(discServer);
                })
            );

        }

        public void Window_UseSelected()
        {
            if (SelectedServer.HasValue)
            {
                if (ServerSelected != null)
                    ServerSelected(SelectedServer.Value);

                if (Close != null)
                    Close();
            }
        }

        public void Window_Cancel()
        {
            if (Close != null)
                Close();
        }

    }
}
