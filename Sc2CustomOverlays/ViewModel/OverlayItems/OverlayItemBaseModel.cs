using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;

using Sc2CustomOverlays.Models.Exceptions;
using System.Windows.Controls;
using Sc2CustomOverlays.Models.MVVMHelpers;
using Sc2CustomOverlays.ViewModel.OverlayVariables;

namespace Sc2CustomOverlays.ViewModel.OverlayItems
{
    public abstract class OverlayItemBaseModel : ObservableModel
    {
        #region Margin
            private Thickness _margin = new Thickness();
            public Thickness Margin
            {
                get { return _margin; }
                set
                {
                    _margin = value;
                    RaisePropertyChanged("Margin");
                }
            }
        #endregion
        #region HorizontalAlign And Default
            protected virtual HorizontalAlignment HorizontalAlignDefault { get { return HorizontalAlignment.Left; } }

            private HorizontalAlignment _horizontalAlign;
            public HorizontalAlignment HorizontalAlign
            {
                get { return _horizontalAlign; }
                set
                {
                    _horizontalAlign = value;
                    RaisePropertyChanged("HorizontalAlign");
                }
            }
        #endregion
        #region VerticalAlign and Default
            protected virtual VerticalAlignment VerticalAlignDefault { get { return VerticalAlignment.Top; } }

            private VerticalAlignment _verticalAlign;
            public VerticalAlignment VerticalAlign
            {
                get { return _verticalAlign; }
                set
                {
                    _verticalAlign = value;
                    RaisePropertyChanged("VerticalAlign");
                }
            }
        #endregion

        protected OverlayItemBaseModel()
        {
            _horizontalAlign = HorizontalAlignDefault;
            _verticalAlign = VerticalAlignDefault;
        }

        public virtual void FromXML(XmlNode xItemNode)
        {

            foreach (XmlAttribute xAttrib in xItemNode.Attributes)
            {
                try
                {
                    switch (xAttrib.LocalName)
                    {
                        case "margin":
                            Margin = (Thickness)(new ThicknessConverter()).ConvertFromString(xAttrib.Value);
                            break;
                        case "halign":
                            HorizontalAlign = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), xAttrib.Value);
                            break;
                        case "valign":
                            VerticalAlign = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), xAttrib.Value);
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

        public abstract void UpdateVariables(Dictionary<string, OverlayVariableBaseModel> variableDictionary);
    }
}