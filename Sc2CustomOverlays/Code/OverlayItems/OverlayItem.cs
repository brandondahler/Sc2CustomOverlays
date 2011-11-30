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
            XmlNode xAttrib = xItemNode.Attributes.GetNamedItem("margin");
            if (xAttrib != null)
                margin = (Thickness)(new ThicknessConverter()).ConvertFromString(xAttrib.Value);

            xAttrib = xItemNode.Attributes.GetNamedItem("halign");
            if (xAttrib != null)
                hAlign = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), xAttrib.Value);

            xAttrib = xItemNode.Attributes.GetNamedItem("valign");
            if (xAttrib != null)
                vAlign = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), xAttrib.Value);

        }

        public virtual void UpdateVariables(Dictionary<string, OverlayVariable> variableDictionary)
        {

        }
    }
}