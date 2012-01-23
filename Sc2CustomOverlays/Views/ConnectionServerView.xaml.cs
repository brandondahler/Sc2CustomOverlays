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
using Sc2CustomOverlays.Models.Networking.StatusConstants;
using Sc2CustomOverlays.Models.Networking.Discovery;
using Sc2CustomOverlays.Models.Networking.Discovery.DiscoveryShared;
using Sc2CustomOverlays.ViewModel;

namespace Sc2CustomOverlays.Views
{
    /// <summary>
    /// Interaction logic for ConnectionServerView.xaml
    /// </summary>
    public partial class ConnectionServerView : UserControl
    {

        public ConnectionServerView()
        {
            InitializeComponent();
        }

        private void pwdServerPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ((ConnectionServerViewModel) this.DataContext).ServerPassword = pwdServerPassword.Password;
        }

        
    }
}
