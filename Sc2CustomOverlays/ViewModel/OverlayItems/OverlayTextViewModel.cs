using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using System.Xml;
using System.Text.RegularExpressions;

using Sc2CustomOverlays.Models.Exceptions;
using Sc2CustomOverlays.Models;
using Sc2CustomOverlays.ViewModel.OverlayVariables;

namespace Sc2CustomOverlays.ViewModel.OverlayItems
{
    class OverlayTextViewModel : OverlayItemBaseModel
    {
        
        // Properties
        #region Justify
            private HorizontalAlignment _justify = HorizontalAlignment.Left;
            public HorizontalAlignment Justify
            {
                get { return _justify; }
                set
                {
                    _justify = value;
                    RaisePropertyChanged("Justify");
                }
            }
        #endregion
        #region Width
            private double _width = double.NaN;
            public double Width
            {
                get { return _width; }
                set
                {
                    _width = value;
                    RaisePropertyChanged("Width");
                }
            }
        #endregion

        #region Text
            private string _text = "";
            public string Text 
            { 
                get { return _text; }
                set
                {
                    _text = value;
                    RaisePropertyChanged("Text");
                }
            }
        #endregion
        #region TextColor
            private SolidColorBrush _textColor = new SolidColorBrush(Colors.Black);
            public SolidColorBrush TextColor
            {
                get { return _textColor; }
                set
                {
                    _textColor = value;
                    RaisePropertyChanged("TextColor");
                }
            }
        #endregion
        #region FontFamily
            private FontFamily _fontFamily = new FontFamily("Segoe UI");
            public FontFamily FontFamily
            {
                get { return _fontFamily; }
                set
                {
                    _fontFamily = value;
                    RaisePropertyChanged("FontFamily");
                }
            }
            #endregion
        #region FontSize
            private double _fontSize = 12;
            public double FontSize
            {
                get { return _fontSize; }
                set
                {
                    _fontSize = value;
                    RaisePropertyChanged("FontSize");
                }
            }
        #endregion

        // Original values
        private string originalColor = null;
        private string originalText = "";

        public override void FromXML(XmlNode xTextNode)
        {
            base.FromXML(xTextNode);

            foreach (XmlAttribute xAttrib in xTextNode.Attributes)
            {
                try
                {
                    switch (xAttrib.LocalName)
                    {
                        case "value":
                            originalText = xAttrib.Value;
                            Text = originalText;
                            break;
                        case "justify":
                            Justify = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), xAttrib.Value);
                            break;
                        case "width":
                            Width = double.Parse(xAttrib.Value);
                            break;
                        case "color":
                            originalColor = xAttrib.Value;

                            if (ColorConverter.ConvertFromString(xAttrib.Value) == null)
                                throw new InvalidXMLValueException("OverlayText", xAttrib.LocalName, InvalidXMLValueException.Reason.InvalidValue);

                            break;
                        case "size":
                            FontSize = double.Parse(xAttrib.Value);
                            break;
                        case "family":
                            FontFamily = new FontFamily(xAttrib.Value);
                            break;
                    }
                } catch (FormatException) {
                    throw new InvalidXMLValueException("OverlayText", xAttrib.LocalName, InvalidXMLValueException.Reason.FormatIncorrect);
                } catch (ArgumentNullException) {
                    throw new InvalidXMLValueException("OverlayText", xAttrib.LocalName, InvalidXMLValueException.Reason.NotSpecified);
                } catch (ArgumentException) {
                    throw new InvalidXMLValueException("OverlayText", xAttrib.LocalName, InvalidXMLValueException.Reason.InvalidValue);
                } catch (OverflowException) {
                    throw new InvalidXMLValueException("OverlayText", xAttrib.LocalName, InvalidXMLValueException.Reason.Overflow);
                }

            }

        }

        private SolidColorBrush GetColorBrush(string colorName)
        {
            try
            {
                return new SolidColorBrush((Color) ColorConverter.ConvertFromString(colorName));
            } catch {
                // Default to black on error
                return new SolidColorBrush(Colors.Black);
            }
        }

        public override void UpdateVariables(Dictionary<string, OverlayVariableBaseModel> variableDictionary)
        {
            Text = VariableEvaluator.ReplaceVariables(originalText, variableDictionary);
            TextColor = GetColorBrush(VariableEvaluator.ReplaceVariables(originalColor, variableDictionary));
        }

    }

}