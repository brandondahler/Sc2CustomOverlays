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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Sc2CustomOverlays.Models.Networking.Control;
using System.Net;
using Sc2CustomOverlays.Models.Networking.StatusConstants;
using Sc2CustomOverlays.Models.Networking.Discovery;
using Sc2CustomOverlays.ViewModel;

namespace Sc2CustomOverlays.Views
{
    /// <summary>
    /// Interaction logic for ConnectionClientView.xaml
    /// </summary>
    public partial class ConnectionClientView : UserControl
    {

        public ConnectionClientView()
        {
            InitializeComponent();
        }

        private void pwdConnectPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ((ConnectionClientViewModel) this.DataContext).ConnectPassword = pwdConnectPassword.Password;
        }
            

    }
}
