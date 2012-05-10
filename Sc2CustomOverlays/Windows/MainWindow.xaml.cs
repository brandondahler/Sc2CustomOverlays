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

using Sc2CustomOverlays.Models;
using Sc2CustomOverlays.Models.Exceptions;
using Sc2CustomOverlays.Models.Networking.Discovery;
using Sc2CustomOverlays.Models.Networking.Discovery.DiscoveryShared;

using System.Net;
using Sc2CustomOverlays.Models.Networking.Control;
using System.IO;

using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using Sc2CustomOverlays.Models.Networking.StatusConstants;



namespace Sc2CustomOverlays.Windows
{
    public delegate void OverlaySettingsChangedHandler(OverlaySettings newOverlaySettings);

    public partial class MainWindow : Window
    {

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
                        /*
                        try
                        {
                            OverlaySettings.Instance.Load(new FileInfo(Path.Combine(OverlaySettings.OverlaysBasePath.FullName, "Starboard\\Starboard.xml")));
                        } catch (OverlayLoadingException ole) {
                            MessageBox.Show("Overlay Loading Exception: " + ole.Message);
                        } catch (Exception ex) {
                            MessageBox.Show("Unhandled exception: " + ex.Message);
                        }
                         */
                    }


                    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
                    {
                        OverlaySettings.Instance.Unload();

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



