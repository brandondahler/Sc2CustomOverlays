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

using Sc2CustomOverlays.Models.Networking.Discovery.DiscoveryShared;

using System.Net;
using System.Reflection;
using System.Collections.ObjectModel;
using Sc2CustomOverlays.ViewModel;
using System.ComponentModel;

namespace Sc2CustomOverlays.Models.Networking.Discovery
{
    
    public partial class FindServers : Window
    {
        // Event fired when a server has been selected and the user has pushed the Use Selected button.    
        public event ServerSelectedHandler ServerSelected;

        private FindServersWindowModel WindowModel = new FindServersWindowModel();

        public FindServers()
        {
            InitializeComponent();

            this.DataContext = WindowModel;
            WindowModel.ServerSelected += new ServerSelectedHandler(WindowModel_ServerSelected);
            WindowModel.Close += new CloseHandler(WindowModel_Close);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowModel.Window_Loaded();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WindowModel.Window_Closing();
        }

        private void WindowModel_ServerSelected(FindServersWindowModel.DiscoveredServer server)
        {
            if (ServerSelected != null)
                ServerSelected(server);
        }

        private void WindowModel_Close()
        {
            this.Close();
        }

        

    }
}
