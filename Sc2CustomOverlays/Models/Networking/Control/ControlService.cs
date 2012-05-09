using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using Sc2CustomOverlays.Models.Networking.Control.Commands;

using System.IO;
using System.ComponentModel;
using Sc2CustomOverlays.Models.Networking.StatusConstants;
using Sc2CustomOverlays.Windows;
using System.Collections.Concurrent;
using Sc2CustomOverlays.Models.Networking.Encryption;
using System.Windows;
using Sc2CustomOverlays.Models.Exceptions;
using Sc2CustomOverlays.ViewModel.OverlayVariables;

namespace Sc2CustomOverlays.Models.Networking.Control
{
    public delegate void DisplayOSSelectionHandler(List<AvailableOverlaySetting> remoteSettings, SettingSelectedHandler handler);
    public delegate void SettingsDownloadedHandler(DirectoryInfo savedDirectory);

    class ControlService : INotifyPropertyChanged
    {
        public event DisplayOSSelectionHandler DisplayOverlaySettingsSelection;
        public event SettingsDownloadedHandler SettingsDownloaded;
        public event PropertyChangedEventHandler PropertyChanged;

        
        #region Member Variables

            // Public members
            #region ConnectedStatus
                // Connection status should be set by server/client
                private ConnectionStatus _connectedStatus = null;
                public ConnectionStatus ConnectedStatus
                {
                    get { return _connectedStatus; }
                    protected set
                    {
                        _connectedStatus = value;
                        RaisePropertyChanged("ConnectedStatus");
                    }
                }
            #endregion

            // Protected members should be autonomous and cleanupable by setting to null.
            #region Connection
                private TcpClient _connection = null;
                protected TcpClient Connection
                {
                    get { return _connection; }
                    set
                    {
                        if (_connection != null)
                        {
                            SendConnectionClose();
                            ConnectedStatus = ConnectionStatus.NotConnected;
                        }

                        _connection = value;
                    }
                }
            #endregion
            #region EncryptedStream
                private Stream _encryptedStream = null;
                protected Stream EncryptedStream 
                { 
                    get { return _encryptedStream; }
                    set
                    {
                        _encryptedStream = value;
                    }
                }
            #endregion

            #region SocketThread
                private Thread _socketThread = null;
                protected Thread SocketThread
                {
                    get { return _socketThread; }
                    set
                    {
                        if (_socketThread != null)
                            _socketThread.Abort();

                        _socketThread = value;
                    }
                }
            #endregion

            #region AvailableRemoteSettings
                private List<AvailableOverlaySetting> _availableRemoteSettings = null;
                protected List<AvailableOverlaySetting> AvailableRemoteSettings
                {
                    get { return _availableRemoteSettings; }
                    set
                    {
                        _availableRemoteSettings = value;
                    }
                }
            #endregion

            protected Dictionary<ControlCommand.Command, EventWaitHandle> ProcessedCommands = new Dictionary<ControlCommand.Command, EventWaitHandle>();

            // Wait handle to wait for an overlay to be selected and event raised.
            private EventWaitHandle overlaySelectedWaitHandle = null;
            private AvailableOverlaySetting remoteSettingToDownload = null;
            
        #endregion

        #region Protected Constructor
            // All constructors should be protected, ControlService cannot exist by itself.

            protected ControlService()
            {
                // Register to be notified when a variable is updated
                OverlaySettings.Instance.VariableUpdated += new VariableUpdatedHandler(OverlaySettings_VariableUpdated);
                
                ConnectedStatus = ConnectionStatus.NotConnected;
            }
        #endregion

        #region Protected Start/Stop Service
            // These functions should be called from all sub-classes

            public virtual void StartService()
            {
                // Cleanup first incase we weren't stopped before start is re-called.
                StopService();

                overlaySelectedWaitHandle = new AutoResetEvent(false);
            }

            public virtual void StopService()
            {
                // Cleanup and cleanly close connection
                Connection = null;

                // Cleanup and abort socket thread.
                SocketThread = null;

                if (overlaySelectedWaitHandle != null)
                {
                    overlaySelectedWaitHandle.Close();
                    overlaySelectedWaitHandle = null;
                }

                // Unblock all threads that might be waiting on ProcessedTracking
                foreach (EventWaitHandle ewh in ProcessedCommands.Values)
                {
                    if (ewh != null)
                    {
                        ewh.Set();
                        ewh.Close();
                    }
                }

                ProcessedCommands.Clear();
                
            }

        #endregion

        #region Protected Event Handlers
            // Virtual event handlers should be called in addition if overriden.
            // Send an update packet
            protected virtual void OverlaySettings_VariableUpdated(OverlayVariableBaseModel ov)
            {
                if (Connection != null && EncryptedStream != null)
                {
                    Dictionary<string, object> updateParams = new Dictionary<string, object>();
                    updateParams["variableName"] = ov.Name;
                    updateParams["variableValue"] = ov.StringValue;

                    try
                    {
                        UpdateVariableCommand.Instance.SendCommand(EncryptedStream, updateParams);
                    } catch (ObjectDisposedException) {
                    } catch (InvalidOperationException) {
                    }
                }
            }

        #endregion

        #region Process Data
            // Processes packets in loop, nothing special
            protected virtual void ProcessData()
            {
                // Process until packet processing failure
                try
                {
                    while (ProcessSinglePacket()) ;
                } catch (CommandException ce) {
                    MessageBox.Show(ce.Message + "\n" + ce.StackTrace);
                }
            }

            // Returns true if processing was successful
            protected virtual bool ProcessSinglePacket(ControlCommand.Command? requiredCommand = null)
            {
                // Alias EncryptedStream as ns for cleaner code
                Stream ns = EncryptedStream;

                byte[] commandBuffer = new byte[1];
                int commandBytesRead = 0;
                
                // Try reading the command byte
                try
                {
                    commandBytesRead = ns.Read(commandBuffer, 0, 1);
                } catch (Exception) {
                    return false;
                }

                // If we read a command byte
                if (commandBytesRead == 1)
                {
                    ControlCommand.Command packetCommand = (ControlCommand.Command)commandBuffer[0];
                    CommandResult cr = null;

                    // Handle incoming commands
                    switch (packetCommand)
                    {
                        case ControlCommand.Command.AvailableSettingsRequest:
                            cr = AvailableSettingsRequestCommand.Instance.HandleCommand(ns);

                            Dictionary<string, object> availableResponseParameters = new Dictionary<string, object>();
                            availableResponseParameters["availableSettings"] = OverlaySettings.GetValidOverlaySettings();
                            bool responseSent = AvailableSettingsResponseCommand.Instance.SendCommand(ns, availableResponseParameters);

                            break;

                        case ControlCommand.Command.AvailableSettingsResponse:
                            cr = AvailableSettingsResponseCommand.Instance.HandleCommand(ns);

                            if (cr.Success)
                                AvailableRemoteSettings = (List<AvailableOverlaySetting>) cr.Data["remoteSettings"];

                            break;

                        case ControlCommand.Command.DownloadSettingRequest:
                            cr = DownloadSettingRequestCommand.Instance.HandleCommand(ns);

                            if (cr.Success)
                            {
                                string settingPath = (string)cr.Data["settingPath"];

                                FileInfo overlaySettingFile = new FileInfo(Path.Combine(OverlaySettings.OverlaysBasePath.FullName, settingPath));
                                DirectoryInfo overlaySettingFolder = overlaySettingFile.Directory;

                                Dictionary<string, object> downloadResponseParameters = new Dictionary<string, object>();
                                downloadResponseParameters["fromDirectory"] = overlaySettingFolder;

                                DownloadSettingResponseCommand.Instance.SendCommand(ns, downloadResponseParameters);
                            }

                            break;

                        case ControlCommand.Command.DownloadSettingResponse:
                            cr = DownloadSettingResponseCommand.Instance.HandleCommand(ns);

                            if (cr.Success)
                            {
                                RaiseSettingsDownloaded((DirectoryInfo)cr.Data["savedDirectory"]);
                            }

                            break;


                        case ControlCommand.Command.OverlaySettingSelect:
                            cr = OverlaySettingSelectCommand.Instance.HandleCommand(ns);

                            if (cr.Success)
                            {
                                if ((bool) cr.Data["remote"])
                                {
                                    Dictionary<string, object> downloadParameters = new Dictionary<string, object>();
                                    downloadParameters["settingPath"] = cr.Data["selectedPath"];
                                    
                                    // Open when download completes
                                    SettingsDownloaded += new SettingsDownloadedHandler(SettingsDownloaded_OpenOnComplete);
                                    remoteSettingToDownload = new AvailableOverlaySetting() { Path = (string)cr.Data["selectedPath"], Local = !((bool)cr.Data["remote"]) };
                                    DownloadSettingRequestCommand.Instance.SendCommand(ns, downloadParameters);

                                } else {
                                    try
                                    {
                                        OverlaySettings.Instance.Load(new FileInfo(Path.Combine(OverlaySettings.OverlaysBasePath.FullName, (string)cr.Data["selectedPath"])));
                                    } catch (OverlayLoadingException ole) {
                                        MessageBox.Show(ole.Message);
                                        return false;
                                    }
                                    OverlaySettingLoadedCommand.Instance.SendCommand(ns);
                                }

                                
                            }

                            break;

                        case ControlCommand.Command.OverlaySettingLoaded:
                            cr = OverlaySettingLoadedCommand.Instance.HandleCommand(ns);
                            break;

                        case ControlCommand.Command.UpdateVariable:

                            cr = UpdateVariableCommand.Instance.HandleCommand(ns);
                            if (cr.Success)
                            {
                                string variableName = (string)cr.Data["variableName"];
                                string variableValue = (string)cr.Data["variableValue"];

                                // Update our dictionary's value of variableName with variableValue
                                OverlaySettings.Instance.UpdateVariableFromNetwork(variableName, variableValue);
                            }

                            break;

                        case ControlCommand.Command.Close:

                            cr = CloseCommand.Instance.HandleCommand(ns);
                            if (cr.Success)
                                return false;

                            break;

                        // Unknown command
                        default:
                            return false;
                    }

                    if (!cr.Success)
                        return false;

                    // Mark packet processed if mutex is valid
                    if (ProcessedCommands.ContainsKey(packetCommand) && ProcessedCommands[packetCommand] != null)
                        ProcessedCommands[packetCommand].Set();

                    // If we processed a command that wasn't required, fail 
                    if (requiredCommand.HasValue && ((ControlCommand.Command)commandBuffer[0]) != requiredCommand.Value)
                        return false;
                }

                return true;
            }

        
        #endregion
        
        #region Utility Functions

            // Can be called on an invalid connection
            private void SendConnectionClose()
            {
                if (Connection != null)
                {
                    if (EncryptedStream != null)
                    {
                        try
                        {
                            CloseCommand.Instance.SendCommand(EncryptedStream, null);
                        } catch (Exception) {
                        }
                    }

                    Connection.Close();
                }
            }

            protected void MarkForProcessedTracking(ControlCommand.Command c)
            {
                ProcessedCommands[c] = new ManualResetEvent(false);
            }

            protected void WaitForProcessedTracking(ControlCommand.Command c)
            {
                EventWaitHandle commandWaitHandle = ProcessedCommands[c];
                commandWaitHandle.WaitOne();
                commandWaitHandle.Close();

                ProcessedCommands.Remove(c);
            }

            // Asyncronously called
            public void DisplayOpenMenuWithRemoteSettings()
            {
                (new Thread(new ThreadStart(delegate ()
                    {
                        // Send request for AvailableSettings and wait for response packet
                        MarkForProcessedTracking(ControlCommand.Command.AvailableSettingsResponse);
                        AvailableSettingsRequestCommand.Instance.SendCommand(EncryptedStream);
                        WaitForProcessedTracking(ControlCommand.Command.AvailableSettingsResponse);

                        // Display selection window and wait for one to be selected.  If it is local it will automatically be loaded
                        DisplayOverlaySettingsSelection(AvailableRemoteSettings, new SettingSelectedHandler(OverlaySettingSelected));
                    }
                ))).Start();
            }

            
            
        #endregion

        #region Event Handlers
            private void OverlaySettingSelected(AvailableOverlaySetting selectedSetting)
            {
                // If our selection wasn't local, request the setting and load it
                if (selectedSetting.Local == false)
                {
                    Dictionary<string, object> downloadRequestParameters = new Dictionary<string, object>();
                    downloadRequestParameters["settingPath"] = selectedSetting.Path;

                    // Open when download completes
                    remoteSettingToDownload = selectedSetting;
                    SettingsDownloaded += new SettingsDownloadedHandler(SettingsDownloaded_OpenOnComplete);

                    DownloadSettingRequestCommand.Instance.SendCommand(EncryptedStream, downloadRequestParameters);
                } else {

                    try 
                    {
                        OverlaySettings.Instance.Load(new FileInfo(Path.Combine(OverlaySettings.OverlaysBasePath.FullName, selectedSetting.Path)));
                    } catch (OverlayLoadingException ole) {
                        MessageBox.Show(ole.Message);
                    }
                }

                // Send our selection
                Dictionary<string, object> selectSettingParameters = new Dictionary<string, object>();
                selectSettingParameters["selectedSetting"] = selectedSetting;
                OverlaySettingSelectCommand.Instance.SendCommand(EncryptedStream, selectSettingParameters);
            }

            private void SettingsDownloaded_OpenOnComplete(DirectoryInfo tempDirectory)
            {
                try
                {
                    OverlaySettings.Instance.Load(new FileInfo(Path.Combine(OverlaySettings.OverlaysTempBasePath.FullName, remoteSettingToDownload.Path)));
                } catch (OverlayLoadingException ole) {
                    MessageBox.Show(ole.Message);
                }

                remoteSettingToDownload = null;

                // Remove self from event
                SettingsDownloaded -= new SettingsDownloadedHandler(SettingsDownloaded_OpenOnComplete);
            }
        #endregion

        #region Raise Event Functions

            protected void RaiseSettingsDownloaded(DirectoryInfo savedDirectory)
            {
                if (SettingsDownloaded != null)
                    SettingsDownloaded(savedDirectory);
            }

            protected void RaisePropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

        #endregion

    }
}
