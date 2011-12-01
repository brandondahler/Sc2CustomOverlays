using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;

namespace Sc2CustomOverlays
{
    abstract class OverlayItem
    {
        protected Thickness? margin = null;
        protected HorizontalAlignment? hAlign = null;
        protected VerticalAlignment? vAlign = null;


        public abstract FrameworkElement GetElement();

        public virtual void FromXML(XmlNode xItemNode)
        {

            foreach (XmlAttribute xAttrib in xItemNode.Attributes)
            {
                switch (xAttrib.LocalName)
                {
                    case "margin":
                        margin = (Thickness)(new ThicknessConverter()).ConvertFromString(xAttrib.Value);
                        break;
                    case "halign":
                        hAlign = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), xAttrib.Value);;
                        break;
                    case "valign":
                        vAlign = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), xAttrib.Value);
                        break;
                }
            }

        }

        public virtual void UpdateVariables(Dictionary<string, OverlayVariable> variableDictionary)
        {

        }
    }
}