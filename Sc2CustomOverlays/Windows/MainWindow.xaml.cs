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
using Microsoft.Win32;
using System.Windows.Controls.Primitives;

using Sc2CustomOverlays.Code;
using Sc2CustomOverlays.Code.OverlayVariables;
using Sc2CustomOverlays.Code.Exceptions;
using Sc2CustomOverlays.Code.Networking.Discovery;
using Sc2CustomOverlays.Code.Networking.Discovery.DiscoveryShared;

using System.Net;
using Sc2CustomOverlays.Code.Networking.Control;
using System.IO;

using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using Sc2CustomOverlays.Code.Networking.StatusConstants;



namespace Sc2CustomOverlays.Windows
{
    public delegate void OverlaySettingsChangedHandler(OverlaySettings newOverlaySettings);

    public partial class MainWindow : Window
    {
        private FindServers fsWindow = null;

        #region ConnectError
            private object ConnectError
            {
                get { return lblConnectError.Content; }
                set 
                {
                    lblConnectError.Content = value;

                    if (value == "")
                        lblConnectError.Visibility = System.Windows.Visibility.Collapsed;
                    else
                        lblConnectError.Visibility = System.Windows.Visibility.Visible;
                }
            }
        #endregion
        #region ListenError
            private object ListenError
            {
                get { return lblListenError.Content; }
                set
                {
                    lblListenError.Content = value;

                    if (value == "")
                        lblListenError.Visibility = System.Windows.Visibility.Collapsed;
                    else
                        lblListenError.Visibility = System.Windows.Visibility.Visible;
                }
            }
        #endregion


        public MainWindow()
        {
            InitializeComponent();

            // Allow client or server to request the OSSelection menu to appear.
            ControlServerService.Instance.DisplayOverlaySettingsSelection += new DisplayOSSelectionHandler(ControlService_DisplayOSSelection);
            ControlClientService.Instance.DisplayOverlaySettingsSelection += new DisplayOSSelectionHandler(ControlService_DisplayOSSelection);
        }

        #region Private Event Handlers

            #region Internal Event Handlers

                #region Window
                    private void Window_Loaded(object sender, RoutedEventArgs e)
                    {
                        try
                        {
                            OverlaySettings.Instance.Load(new FileInfo(Path.Combine(OverlaySettings.OverlaysBasePath.FullName, "Starboard\\Starboard.xml")));
                        } catch (OverlayLoadingException ole) {
                            MessageBox.Show("Overlay Loading Exception: " + ole.Message);
                        } catch (Exception ex) {
                            MessageBox.Show("Unhandled exception: " + ex.Message);
                        }
                    }


                    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
                    {
                        OverlaySettings.Instance.Unload();

                        if (fsWindow != null)
                            fsWindow.Close();

                        DiscoveryServerService.Instance.StopService();

                        ControlClientService.Instance.StopService();
                        ControlServerService.Instance.StopService();
                    }
                #endregion

                #region Menu
                    private void menuOpen_Click(object sender, RoutedEventArgs e)
                    {

                        if (ControlClientService.Instance.ConnectedStatus != ConnectionStatus.NotConnected)
                        {
                            ControlClientService.Instance.DisplayOpenMenuWithRemoteSettings();
                        } else {
                            if (ControlServerService.Instance.ConnectedStatus != ConnectionStatus.NotConnected)
                            {
                                ControlServerService.Instance.DisplayOpenMenuWithRemoteSettings();
                            } else {
                                SelectOverlaySettings sos = new SelectOverlaySettings() { Owner = this, AvailableSettings = OverlaySettings.GetValidOverlaySettings(), LocalOnly = true };
                                sos.SettingSelected += new SettingSelectedHandler(SelectOverlaySettings_OverlaySettingSelected);
                                sos.ShowDialog();
                            }
                        }

            
                    }

                    private void menuExit_Click(object sender, RoutedEventArgs e)
                    {
                        this.Close();
                    }
                #endregion

                #region Overlay Controls
                    private void btnShowOverlays_Click(object sender, RoutedEventArgs e)
                    {
                        OverlaySettings.Instance.ToggleOverlayVisibility();
                    }
                #endregion

                #region Client

                    private void btnDiscoverServers_Click(object sender, RoutedEventArgs e)
                    {

                        fsWindow = new FindServers() { Owner = this };
                        fsWindow.ServerSelected += FindServers_ServerSelected;
                        fsWindow.ShowDialog();

                        fsWindow = null;
                    }

                    private void btnConnect_Click(object sender, RoutedEventArgs e)
                    {
                        ConnectError = "";

                        if (ControlClientService.Instance.ConnectedStatus == ConnectionStatus.NotConnected)
                        {
                            IPAddress ipAddress;
                            ushort port;
                            try 
                            {
                                ipAddress = IPAddress.Parse(txtConnectIPAddress.Text);
                            } catch (Exception) {
                                ConnectError = "Invalid IP address format given.";
                                return;
                            }

                            try 
                            {
                                port = ushort.Parse(txtConnectPort.Text);
                            } catch (Exception) {
                                ConnectError = "Invalid port given.";
                                return;
                            }

                            if (pwdConnectPassword.Password == "")
                            {
                                ConnectError = "Password is required.";
                                return;
                            }

                            ControlClientService.Instance.StartService(ipAddress, port, pwdConnectPassword.Password);
                        } else {
                            ControlClientService.Instance.StopService();
                        }
                    }

                #endregion
                
                #region Server

                    private void chkMakeDiscoverable_CheckChanged(object sender, RoutedEventArgs e)
                    {
                        if (chkMakeDiscoverable.IsChecked.HasValue)
                        {
                            lblDiscoverName.Visibility = (chkMakeDiscoverable.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed);
                            txtDiscoverName.Visibility = (chkMakeDiscoverable.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed);
                        }
                    }

                    private void btnStartListening_Click(object sender, RoutedEventArgs e)
                    {
                        ListenError = "";
                        if (ControlServerService.Instance.ListeningStatus == ListenStatus.NotListening)
                        {
                            ushort port;

                            try
                            {
                                port = ushort.Parse(txtServerPort.Text);
                            } catch (Exception) {
                                ListenError = "Invalid port given.";
                                return;
                            }

                            if (pwdServerPassword.Password == "")
                            {
                                ListenError = "Password is required.";
                                return;
                            }

                            if (chkMakeDiscoverable.IsChecked.Value)
                                DiscoveryServerService.Instance.StartService(new DiscoveryServerInfo() { Name = txtDiscoverName.Text, Port = port });

                            ControlServerService.Instance.StartService(port, pwdServerPassword.Password);

                        } else {

                            if (DiscoveryServerService.Instance.Discoverable)
                                DiscoveryServerService.Instance.StopService();

                            ControlServerService.Instance.StopService();
                        }
                    }

                #endregion

            #endregion

            #region External Event Handlers

                void SelectOverlaySettings_OverlaySettingSelected(AvailableOverlaySetting selectedSetting)
                {
                    if (selectedSetting.Local != true)
                        throw new ArgumentException();

                    try
                    {
                        OverlaySettings.Instance.Load(new FileInfo(Path.Combine(OverlaySettings.OverlaysBasePath.FullName, selectedSetting.Path)));
                    } catch (OverlayLoadingException ole) {
                        MessageBox.Show("Overlay Loading Exception: " + ole.Message);
                    } catch (Exception ex) {
                        MessageBox.Show("Unhandled exception: " + ex.Message);
                    }
                }

                private void FindServers_ServerSelected(DiscoveredServer server)
                {
                    txtConnectIPAddress.Text = server.Ip.ToString();
                    txtConnectPort.Text = server.Port.ToString();
                }

                private void ControlService_DisplayOSSelection(List<AvailableOverlaySetting> remoteSettings, SettingSelectedHandler handler)
                {
                    // Must be called via dispatcher, this event is called from a network thread
                    Dispatcher.Invoke(new Action(delegate()
                        {
                            SelectOverlaySettings sosWindow = new SelectOverlaySettings() { AvailableSettings = remoteSettings, Owner = this };

                            if (handler != null)
                                sosWindow.SettingSelected += handler;

                            sosWindow.AvailableSettings.AddRange(OverlaySettings.GetValidOverlaySettings());
                            sosWindow.ShowDialog();
                        }
                    ));
                }  

            #endregion

        #endregion

    }
}



