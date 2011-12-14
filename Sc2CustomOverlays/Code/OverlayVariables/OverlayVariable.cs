using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Xml;
using System.Windows;
using Sc2CustomOverlays.Code.Exceptions;

namespace Sc2CustomOverlays.Code.OverlayVariables
{
    public delegate void UpdatedEventHandler();

    public partial class OverlayVariable : UserControl
    {

        public event UpdatedEventHandler Updated;
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
        public string Group { get { return group; } }
        public virtual string Value { get { throw new NotImplementedException(); } }

        protected string name = null;
        protected string label = null;
        protected string group = null;

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
                        group = vNodeAttrib.Value;
                        break;
                }
            }
        }

        public virtual OverlayControlsContainer GetElements()
        {
            OverlayControlsContainer occ = new OverlayControlsContainer();

            occ.label = new Label() { Content = Label };
            occ.save = new Button() { Content = "Save", Padding = new Thickness(15, 0, 15, 0) };
            occ.reset = new Button() { Content = "Reset", Padding = new Thickness(15, 0, 15, 0) };

            occ.modifier = this;

            return occ;
        }

        protected void RaiseUpdated()
        {
            if (Updated != null)
                Updated();
        }

        public static Dictionary<string, OverlayVariable> ProcessVariables(XmlNode vlNode, IEnumerable<string> validGroups, string startDirectory)
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
                        ov = new OverlayItemSelector(startDirectory);
                        break;
                }

                if (ov != null)
                {
                    try
                    {
                        ov.FromXML(vNode);
                    } catch (InvalidXMLValueException ex) {
                        MessageBox.Show(ex.Message);
                        throw new VariableParseException(VariableParseFailure.InvalidXML);
                    }

                    if (ov.group != null && !validGroups.Contains(ov.group))
                        ov.group = null;

                    try
                    {
                        variableDictionary.Add(ov.VariableName, ov);
                    } catch (ArgumentException) {
                        if (ov.VariableName == null)
                        {
                            throw new VariableParseException(VariableParseFailure.NullVariable);
                        } else {
                            throw new VariableParseException(VariableParseFailure.DuplicateVariable);
                        }
                    }
                }
            }
            return variableDictionary;
        }
    }

}
