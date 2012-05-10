using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using Sc2CustomOverlays.Models.Exceptions;
using System.Windows;
using System.IO;
using System.ComponentModel;
using Sc2CustomOverlays.Windows;
using Sc2CustomOverlays.ViewModel;
using Sc2CustomOverlays.ViewModel.OverlayVariables;

namespace Sc2CustomOverlays.Models
{
    public delegate void OverlaySettingsLoadedHandler();

    public class OverlaySettings : INotifyPropertyChanged
    {
        #region Instance
            private static OverlaySettings _instance = new OverlaySettings();
            public static OverlaySettings Instance { get { return _instance; } }
        #endregion

        // Public Static Constants
        #region OverlayBasePath
            private static DirectoryInfo _overlaysBasePath = new DirectoryInfo("Overlays\\");
            public static DirectoryInfo OverlaysBasePath { get { return _overlaysBasePath; } }
        #endregion
        #region OverlaysTempBasePath
            private static DirectoryInfo _overlaysTempBasePath = new DirectoryInfo("TempOverlays\\");
            public static DirectoryInfo OverlaysTempBasePath { get { return _overlaysTempBasePath; } }
        #endregion

        // Public Events
        public event PropertyChangedEventHandler PropertyChanged;

        public event OverlaySettingsLoadedHandler OverlaySettingsLoaded;
        public event VariableUpdatedHandler VariableUpdated;

        // Public Properties
        public string Name { get { return diskLocation.Directory.Name; } }
        public string VersionName { get { return diskLocation.Name; } }
        public FileInfo Location { get { return diskLocation; } }
        public Dictionary<string, OverlayVariableGroup> VariableGroupsLookup { get { return variableGroups; } }

        // Public Notifiable Properties
        #region OverlayControls
            public IEnumerable<OverlayVariableBaseModel> OverlayControls { get { return variableDictionary.Values; } }
        #endregion

        // Private members
        private bool overlaysVisible = false;
        private FileInfo diskLocation = null;
        private List<OverlayWindow> overlayList = new List<OverlayWindow>();
        private Dictionary<string, OverlayVariableBaseModel> variableDictionary = new Dictionary<string, OverlayVariableBaseModel>();
        private Dictionary<string, OverlayVariableGroup> variableGroups = new Dictionary<string, OverlayVariableGroup>();

        protected OverlaySettings() { }
        
        #region Public Member Functions

            #region Load/Unload
                public void Load(FileInfo settingsLocation)
                {
                    // Always load on the dispatcher thread.
                    Application.Current.Dispatcher.Invoke(new Action(delegate()
                        {

                            // The settings location cannot be null or non-existent
                            if (settingsLocation == null || !settingsLocation.Exists)
                                throw new ArgumentNullException();

                            // Clean up incase there was a previously loaded setting
                            //  Don't do a full unload so that there are no race conditions with other threads using the public properties.
                            Unload(false);

                            // Record the diskLocation and load from XML
                            diskLocation = settingsLocation;
                            FromXML(diskLocation);

                            // Raise event if there are any subscribers
                            if (OverlaySettingsLoaded != null)
                                OverlaySettingsLoaded();
                        })
                    );
                }
                
                // fullUnload invalidates OverlaySettings completely, if invalidated, race conditions can occur.
                public void Unload(bool fullUnload = true)
                {
                    // Close overlays and clear the list

                    foreach (OverlayWindow o in overlayList)
                    {
                        o.Dispatcher.Invoke(new Action(delegate()
                            {
                                o.AllowClose = true;
                                o.Close();
                            })
                        );
                    }
                    overlayList.Clear();

                    // Reset setting-specific variables
                    overlaysVisible = false;

                    // Clear variable dictionary and variable groups dictionary
                    variableDictionary.Clear();
                    variableGroups.Clear();

                    if (fullUnload)
                        diskLocation = null;
                }
            #endregion

            #region Overlay Visibility Functions
                public void ToggleOverlayVisibility()
                {
                    if (overlaysVisible)
                        OverlaySettings.Instance.HideOverlays();
                    else
                        OverlaySettings.Instance.ShowOverlays();
                }

                public void ShowOverlays()
                {
                    foreach (OverlayWindow o in overlayList)
                        o.Show();

                    overlaysVisible = true;
                }

                public void HideOverlays()
                {
                    foreach (OverlayWindow o in overlayList)
                        o.Hide();

                    overlaysVisible = false;
                }
            #endregion

            public void UpdateVariableFromNetwork(string variableName, string variableValue)
            {
                variableDictionary[variableName].StringValue = variableValue;
            }

        #endregion

        #region Public Static Functions

            public static List<AvailableOverlaySetting> GetValidOverlaySettings()
            {
                List<AvailableOverlaySetting> availableSettings = new List<AvailableOverlaySetting>();
                Uri baseUri = new Uri(OverlaysBasePath.FullName);

                if (!OverlaysBasePath.Exists)
                    OverlaysBasePath.Create();

                foreach (DirectoryInfo settingsFolder in OverlaysBasePath.GetDirectories())
                {
                    foreach (FileInfo overlaySettingFile in settingsFolder.GetFiles("*.xml"))
                    {
                        AvailableOverlaySetting aos = new AvailableOverlaySetting()
                        {
                            Local = true,
                            Name = settingsFolder.Name + ": " + Path.GetFileNameWithoutExtension(overlaySettingFile.Name),
                            Path = baseUri.MakeRelativeUri(new Uri(overlaySettingFile.FullName)).ToString(),
                            IsCurrent = (Instance.diskLocation != null ? (Instance.diskLocation.FullName == overlaySettingFile.FullName) : false)
                        };

                        availableSettings.Add(aos);

                    }
                }

                return availableSettings;
            }

        #endregion

        #region Private Member Functions

            private void FromXML(FileInfo file)
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(file.FullName);

                XmlNode osNode = xDoc.SelectSingleNode("/OverlaySettings");

                if (osNode == null)
                    throw new OverlayLoadingException(OverlayLoadingException.Reason.InvalidXMLFormat);


                // Process groups
                variableGroups.Clear();
                XmlNodeList glNode = osNode.SelectNodes("Groups/Group");
                if (glNode != null)
                {
                    foreach (XmlNode gNode in glNode)
                    {
                        string groupName = gNode.Attributes.GetNamedItem("name").Value;
                        string groupLabel = gNode.Attributes.GetNamedItem("label").Value;
                        variableGroups.Add(groupName, new OverlayVariableGroup() { GroupName = groupName, GroupLabel = groupLabel });
                    }
                }

                variableGroups.Add("", new OverlayVariableGroup() { GroupName = "", GroupLabel = "" });

                // Process variables
                variableDictionary.Clear();
                XmlNode vlNode = osNode.SelectSingleNode("Variables");
                if (vlNode != null)
                {
                    try
                    {
                        variableDictionary = OverlayVariableBaseModel.ProcessVariables(vlNode, diskLocation.Directory);
                    } catch (VariableParseException ex) {
                        MessageBox.Show(ex.Message);
                        throw new OverlayLoadingException(OverlayLoadingException.Reason.VariableProcessing);
                    }
                }

                // Add VariableUpdated handler to each variable's Updated event
                foreach (OverlayVariableBaseModel ov in variableDictionary.Values)
                    ov.VariableUpdated += new VariableUpdatedHandler(OverlayVariable_VariableUpdated);

                // Let everyone know the overlay controls have changed
                RaisePropertyChanged("OverlayControls");

                // Process overlays
                XmlNodeList oNodes = osNode.SelectNodes("Overlays/Overlay");
                if (oNodes != null)
                {
                    foreach (XmlNode oNode in oNodes)
                    {
                        OverlayWindow o = new OverlayWindow();
                        try
                        {
                            ((OverlayWindowModel) o.DataContext).FromXML(oNode);
                        } catch (OverlayCreationException ex) {
                            MessageBox.Show(ex.Message);
                            throw new OverlayLoadingException(OverlayLoadingException.Reason.OverlayCreation);
                        }
                        ((OverlayWindowModel) o.DataContext).UpdateVariables(variableDictionary);
                        overlayList.Add(o);
                    }
                }

            }

            private void OverlayVariable_VariableUpdated(OverlayVariableBaseModel sender)
            {
                // Call update variables on each overlay
                Application.Current.Dispatcher.Invoke(new Action(delegate()
                    {
                        foreach (OverlayWindow o in overlayList)
                            ((OverlayWindowModel) o.DataContext).UpdateVariables(variableDictionary);

                    })
                );

                
                if (VariableUpdated != null)
                    VariableUpdated(sender);

                    
            }

            private void RaisePropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

        #endregion
    }
    
}
