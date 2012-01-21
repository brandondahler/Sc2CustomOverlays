using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;

using Sc2CustomOverlays.Code.OverlayVariables;
using Sc2CustomOverlays.Code.Exceptions;

namespace Sc2CustomOverlays.Code.OverlayItems
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
                try
                {
                    switch (xAttrib.LocalName)
                    {
                        case "margin":
                            margin = (Thickness)(new ThicknessConverter()).ConvertFromString(xAttrib.Value);
                            break;
                        case "halign":
                            hAlign = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), xAttrib.Value);
                            break;
                        case "valign":
                            vAlign = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), xAttrib.Value);
                            break;
                    }
                } catch (NotSupportedException) {
                    throw new InvalidXMLValueException("OverlayItem", xAttrib.LocalName, InvalidXMLValueException.Reason.FormatIncorrect);
                } catch (ArgumentNullException) {
                    throw new InvalidXMLValueException("OverlayItem", xAttrib.LocalName, InvalidXMLValueException.Reason.InvalidValue);
                } catch (ArgumentException) {
                    throw new InvalidXMLValueException("OverlayItem", xAttrib.LocalName, InvalidXMLValueException.Reason.InvalidValue);
                } catch (OverflowException) {
                    throw new InvalidXMLValueException("OverlayItem", xAttrib.LocalName, InvalidXMLValueException.Reason.InvalidValue);
                }
            }

        }

        public virtual void UpdateVariables(Dictionary<string, OverlayVariable> variableDictionary)
        {

        }
    }
}