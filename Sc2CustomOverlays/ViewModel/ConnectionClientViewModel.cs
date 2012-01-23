// -----------------------------------------------------------------------
// <copyright file="ConnectionClientViewModel.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Sc2CustomOverlays.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using System.Windows.Input;
    using Sc2CustomOverlays.Models.MVVMHelpers;
    using Sc2CustomOverlays.Models.MVVMHelpers.Commands;
    using System.Windows;
    using Sc2CustomOverlays.Models.Networking.Discovery;
    using Sc2CustomOverlays.Models.Networking.Control;
    using System.Net;
    using Sc2CustomOverlays.Models.Networking.StatusConstants;
    using System.ComponentModel;
    using System.Security;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ConnectionClientViewModel : ObservableModel
    {
        // Commands
        public ICommand FindServer { get { return new DelegateCommand(View_FindServer); } }
        public ICommand Connect { get { return new DelegateCommand(View_Connect); } }

        // Properties
        #region ConnectIPAddress
            private string _connectIPAddress = "";
            public string ConnectIPAddress
            {
                get  { return _connectIPAddress; }
                set
                {
                    _connectIPAddress = value;
                    RaisePropertyChanged("ConnectIPAddress");
                }
            }
        #endregion
        #region ConnectPort
            private string _connectPort = "9999";
            public string ConnectPort
            {
                get { return _connectPort; }
                set
                {
                    _connectPort = value;
                    RaisePropertyChanged("ConnectPort");
                }
            }
        #endregion
        #region ConnectPassword
            private string _connectPassword = null;
            public string ConnectPassword
            {
                get { return _connectPassword; }
                set
                {
                    _connectPassword = value;
                    RaisePropertyChanged("ConnectPassword");
                }
            }
        #endregion
        #region ConnectError
            private string _connectError = "";
            public string ConnectError
            {
                get { return _connectError; }
                set
                {
                    _connectError = value;
                    RaisePropertyChanged("ConnectError");
                }
            }
        #endregion
        #region ConnectedStatus
            public string ConnectedStatus
            {
                get { return ControlClientService.Instance.ConnectedStatus.ToString(); }
            }
        #endregion

        private FindServers fsWindow = null;


        public ConnectionClientViewModel()
        {
            ControlClientService.Instance.PropertyChanged += new PropertyChangedEventHandler(ControlClientService_PropertyChanged);
        }

        ~ConnectionClientViewModel()
        {
            if (fsWindow != null)
                fsWindow.Close();
        }

        private bool InputsValid()
        {
            try
            {
                IPAddress.Parse(ConnectIPAddress);
            } catch (Exception) {
                ConnectError = "Invalid IP Address entered.";
                return false;
            }

            try 
            {
                ushort.Parse(ConnectPort);
            } catch (Exception) {
                ConnectError = "Invalid port entered.";
                return false;
            }

            if (ConnectPassword == null || ConnectPassword.Length == 0)
            {
                ConnectError = "Password is required.";
                return false;
            }

            return true;
        }

        public void View_FindServer()
        {
            fsWindow = new FindServers() { Owner = Application.Current.MainWindow };
            fsWindow.ServerSelected += FindServers_ServerSelected;
            fsWindow.ShowDialog();

            fsWindow = null;
        }

        void View_Connect()
        {
            ConnectError = "";

            if (ControlClientService.Instance.ConnectedStatus == ConnectionStatus.NotConnected)
            {
                if (InputsValid())
                {
                    ControlClientService.Instance.StartService(IPAddress.Parse(ConnectIPAddress), 
                                                               ushort.Parse(ConnectPort), 
                                                               ConnectPassword);
                }
            } else {
                ControlClientService.Instance.StopService();
            }
        }

        private void FindServers_ServerSelected(FindServersWindowModel.DiscoveredServer server)
        {
            ConnectIPAddress = server.Ip.ToString();
            ConnectPort = server.Port.ToString();
        }

        private void ControlClientService_PropertyChanged(object sender, PropertyChangedEventArgs pcea)
        {
            if (pcea.PropertyName == "ConnectedStatus")
                RaisePropertyChanged("ConnectedStatus");
        }
    }
}
