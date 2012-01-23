using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Xml;
using System.Windows;
using Sc2CustomOverlays.Models.Exceptions;
using System.ComponentModel;
using System.IO;
using Sc2CustomOverlays.Models.MVVMHelpers;
using Sc2CustomOverlays.Models;

namespace Sc2CustomOverlays.ViewModel.OverlayVariables
{
    public delegate void VariableUpdatedHandler(OverlayVariableBaseModel sender);

    public abstract class OverlayVariableBaseModel : ObservableModel
    {
        public event VariableUpdatedHandler VariableUpdated;

        #region Name
            protected string name = null;
            public string Name { get { return name; } }
        #endregion
        #region Label
            protected string label = null;
            public string Label
            {
                get
                {
                    if (label == null)
                        return name;

                    return label;
                }
            }
        #endregion
        #region StringValue
            public abstract string StringValue { get; set; }
        #endregion

        #region Group
            protected OverlayVariableGroup group = null;
            public OverlayVariableGroup Group { get { return group; } }
        #endregion

        private string lastStringValue = null;

        public virtual void FromXML(XmlNode vNode)
        {
            foreach (XmlAttribute vNodeAttrib in vNode.Attributes)
            {
                switch (vNodeAttrib.LocalName)
                {
                    case "name":
                        name = vNodeAttrib.Value;
                        break;
                    case "label":
                        label = vNodeAttrib.Value;
                        break;
                    case "group":
                        string groupName = vNodeAttrib.Value;

                        if (OverlaySettings.Instance.VariableGroupsLookup.ContainsKey(groupName))
                            group = OverlaySettings.Instance.VariableGroupsLookup[groupName];

                        break;
                }
            }

            if (group == null)
                group = OverlaySettings.Instance.VariableGroupsLookup[""];
        }

        //public abstract void FromNetwork(string value);

        /*
        public virtual OverlayControlsContainer GetElements()
        {
            OverlayControlsContainer occ = new OverlayControlsContainer();

            occ.Label = new Label() { Content = Label };
            occ.Save = new Button() { Content = "Save", Padding = new Thickness(15, 0, 15, 0) };
            occ.Reset = new Button() { Content = "Reset", Padding = new Thickness(15, 0, 15, 0) };

            occ.Modifier = this;

            occ.Group = group;

            return occ;
        }*/

        protected virtual void RaiseUpdated()
        {
            // Only raise updated if value has actually changed.
            if (lastStringValue == null || lastStringValue != StringValue)
            {
                RaisePropertyChanged("StringValue");

                if (VariableUpdated != null)
                    VariableUpdated(this);

                lastStringValue = StringValue;
            }
        }


        public static Dictionary<string, OverlayVariableBaseModel> ProcessVariables(XmlNode vlNode, DirectoryInfo startDirectory)
        {
            Dictionary<string, OverlayVariableBaseModel> variableDictionary = new Dictionary<string, OverlayVariableBaseModel>();

            foreach (XmlNode vNode in vlNode.ChildNodes)
            {
                OverlayVariableBaseModel ov = null;
                switch (vNode.LocalName)
                {
                    case "Counter":
                        ov = new OverlayCounterViewModel();
                        break;

                    case "DropDown":
                        ov = new OverlayDropDownViewModel();
                        break;

                    case "String":
                        ov = new OverlayStringViewModel();
                        break;

                    case "ItemSelector":
                        ov = new OverlayItemSelectorViewModel();
                        break;
                }

                if (ov != null)
                {
                    try
                    {
                        ov.FromXML(vNode);
                    } catch (InvalidXMLValueException ex) {
                        MessageBox.Show(ex.Message);
                        throw new VariableParseException(VariableParseException.Reason.InvalidXML);
                    }

                    try
                    {
                        variableDictionary.Add(ov.Name, ov);
                    } catch (ArgumentException) {
                        if (ov.Name == null)
                        {
                            throw new VariableParseException(VariableParseException.Reason.NullVariable);
                        } else {
                            throw new VariableParseException(VariableParseException.Reason.DuplicateVariable);
                        }
                    }
                }
            }
            return variableDictionary;
        }
    }

}
