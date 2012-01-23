// -----------------------------------------------------------------------
// <copyright file="ConnectionServerViewModel.cs" company="">
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
    using Sc2CustomOverlays.Models.Networking.Control;
    using Sc2CustomOverlays.Models.Networking.StatusConstants;
    using Sc2CustomOverlays.Models.Networking.Discovery;
    using Sc2CustomOverlays.Models.Networking.Discovery.DiscoveryShared;
    using System.Windows.Input;
    using Sc2CustomOverlays.Models.MVVMHelpers.Commands;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ConnectionServerViewModel : ObservableModel
    {
        // Commands
        public ICommand StartListening { get { return new DelegateCommand(View_StartListening); } }

        // Properties
        #region ServerPort
            private string _serverPort = "9999";
            public string ServerPort
            {
                get { return _serverPort; }
                set
                {
                    _serverPort = value;
                    RaisePropertyChanged("ServerPort");
                }
            }
        #endregion
        #region ServerPassword
            private string _serverPassword = "";
            public string ServerPassword
            {
                get { return _serverPassword; }
                set
                {
                    _serverPassword = value;
                    RaisePropertyChanged("ServerPassword");
                }
            }
        #endregion
        #region DiscoverableChecked
            private bool _discoverableChecked = false;
            public bool DiscoverableChecked
            {
                get { return _discoverableChecked; }
                set
                {
                    _discoverableChecked = value;
                    RaisePropertyChanged("DiscoverableChecked");
                }
            }
        #endregion
        #region DiscoverName
            private string _discoverName = "Unnamed Sc2CustomOverlay";
            public string DiscoverName
            {
                get { return _discoverName; }
                set
                {
                    _discoverName = value;
                    RaisePropertyChanged("DiscoverName");
                }
            }
        #endregion
        #region ListenError
            private string _listenError = "";
            public string ListenError
            {
                get { return _listenError; }
                set
                {
                    _listenError = value;
                    RaisePropertyChanged("ListenError");
                }
            }
        #endregion

        private void View_StartListening()
        {
            ListenError = "";

            if (ControlServerService.Instance.ListeningStatus == ListenStatus.NotListening)
            {
                ushort port;

                try
                {
                    port = ushort.Parse(ServerPort);
                } catch (Exception) {
                    ListenError = "Invalid port given.";
                    return;
                }

                if (ServerPassword == "")
                {
                    ListenError = "Password is required.";
                    return;
                }

                if (DiscoverableChecked)
                    DiscoveryServerService.Instance.StartService(new DiscoveryServerInfo() { Name = DiscoverName, Port = port });

                ControlServerService.Instance.StartService(port, ServerPassword);

            } else {

                if (DiscoveryServerService.Instance.Discoverable)
                    DiscoveryServerService.Instance.StopService();

                ControlServerService.Instance.StopService();
            }
        }
    }
}
