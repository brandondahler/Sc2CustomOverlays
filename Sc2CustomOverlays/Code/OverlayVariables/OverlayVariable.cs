using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Controls;

namespace Sc2CustomOverlays
{
    public delegate void UpdatedEventHandler();

    public abstract class OverlayVariable
    {
        public event UpdatedEventHandler Updated;
        public string Name { get { return name; } }

        public string Label
        {
            get
            {
                if (label == null)
                    return name;

                return label;
            }
        }

        public abstract string Value { get; }


        protected string name = null;
        protected string label = null;


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
                }
            }
        }

        public virtual OverlayControlsContainer GetElements()
        {
            OverlayControlsContainer occ = new OverlayControlsContainer();
            
            occ.label = new Label() { Content = Label};
            occ.save = new Button() { Content = "Save", Padding = new Thickness(15, 0, 15, 0) };
            occ.reset = new Button() { Content = "Reset", Padding = new Thickness(15, 0, 15, 0) };

            return occ;
        }

        protected void RaiseUpdated()
        {
            if (Updated != null)
                Updated();
        }

    }

}
