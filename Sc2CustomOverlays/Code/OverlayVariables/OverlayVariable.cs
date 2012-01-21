using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Xml;
using System.Windows;
using Sc2CustomOverlays.Code.Exceptions;
using System.ComponentModel;
using System.IO;

namespace Sc2CustomOverlays.Code.OverlayVariables
{
    public delegate void VariableUpdatedHandler(OverlayVariable sender);

    public partial class OverlayVariable : UserControl, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public event VariableUpdatedHandler VariableUpdated;
        public string VariableName { get { return name; } }
        public string Label
        {
            get
            {
                if (label == null)
                    return name;

                return label;
            }
        }
        public OverlayVariableGroup Group { get { return group; } }
        public virtual string Value { get { throw new NotImplementedException(); } }
        private string lastValue = null;

        protected string name = null;
        protected string label = null;
        protected OverlayVariableGroup group = null;

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

        public virtual void FromNetwork(string value)
        {
            throw new NotImplementedException();
        }

        public virtual OverlayControlsContainer GetElements()
        {
            OverlayControlsContainer occ = new OverlayControlsContainer();

            occ.Label = new Label() { Content = Label };
            occ.Save = new Button() { Content = "Save", Padding = new Thickness(15, 0, 15, 0) };
            occ.Reset = new Button() { Content = "Reset", Padding = new Thickness(15, 0, 15, 0) };

            occ.Modifier = this;

            occ.Group = group;

            return occ;
        }

        protected void RaiseUpdated()
        {
            // Only raise updated if value has actually changed.
            if (lastValue == null || lastValue != Value)
            {
                RaisePropertyChanged("Value");

                if (VariableUpdated != null)
                    VariableUpdated(this);

                lastValue = Value;
            }
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        public static Dictionary<string, OverlayVariable> ProcessVariables(XmlNode vlNode, DirectoryInfo startDirectory)
        {
            Dictionary<string, OverlayVariable> variableDictionary = new Dictionary<string, OverlayVariable>();

            foreach (XmlNode vNode in vlNode.ChildNodes)
            {
                OverlayVariable ov = null;
                switch (vNode.LocalName)
                {
                    case "Counter":
                        ov = new OverlayCounter();
                        break;

                    case "DropDown":
                        ov = new OverlayDropDown();
                        break;

                    case "String":
                        ov = new OverlayString();
                        break;

                    case "ItemSelector":
                        ov = new OverlayItemSelector(startDirectory.FullName + "\\");
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
                        variableDictionary.Add(ov.VariableName, ov);
                    } catch (ArgumentException) {
                        if (ov.VariableName == null)
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
