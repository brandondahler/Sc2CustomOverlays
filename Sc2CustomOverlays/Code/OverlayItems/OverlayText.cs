using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using System.Xml;
using System.Text.RegularExpressions;

using Sc2CustomOverlays.Code.OverlayVariables;
using Sc2CustomOverlays.Code.Exceptions;

namespace Sc2CustomOverlays.Code.OverlayItems
{
    class OverlayText : OverlayItem
    {
        private string originalText = "";
        private string text = "";

        protected HorizontalAlignment? justify = null;
        protected double? width = null;

        private string originalColor = null;
        private string textColor = null;
        private int? fontSize = null;
        private FontFamily fontFamily = null;

        
        
        

        private Label MyLabel = null;

        public override System.Windows.FrameworkElement GetElement()
        {
            MyLabel = new Label()
            {
                Content = text,
                HorizontalAlignment = (hAlign.HasValue ? hAlign.Value : HorizontalAlignment.Left),
                VerticalAlignment = (vAlign.HasValue ? vAlign.Value : VerticalAlignment.Top),
                HorizontalContentAlignment = (justify.HasValue ? justify.Value : HorizontalAlignment.Left)
            };

            if (margin.HasValue) MyLabel.Margin = margin.Value;
            if (width.HasValue) MyLabel.Width = width.Value;

            if (textColor != null) MyLabel.Foreground = GetColorBrush(textColor);
            if (fontSize.HasValue) MyLabel.FontSize = fontSize.Value;
            if (fontFamily != null) MyLabel.FontFamily = fontFamily;

            return MyLabel;
        }

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
                            text = originalText;
                            break;
                        case "justify":
                            justify = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), xAttrib.Value);
                            break;
                        case "width":
                            width = double.Parse(xAttrib.Value);
                            break;
                        case "color":
                            originalColor = xAttrib.Value;
                            textColor = originalColor;

                            if (ColorConverter.ConvertFromString(xAttrib.Value) == null)
                                throw new InvalidXMLValueException("OverlayText", xAttrib.LocalName, InvalidValueReason.InvalidValue);

                            break;
                        case "size":
                            fontSize = int.Parse(xAttrib.Value);
                            break;
                        case "family":
                            fontFamily = new FontFamily(xAttrib.Value);
                            break;
                    }
                } catch (FormatException) {
                    throw new InvalidXMLValueException("OverlayText", xAttrib.LocalName, InvalidValueReason.FormatIncorrect);
                } catch (ArgumentNullException) {
                    throw new InvalidXMLValueException("OverlayText", xAttrib.LocalName, InvalidValueReason.NotSpecified);
                } catch (ArgumentException) {
                    throw new InvalidXMLValueException("OverlayText", xAttrib.LocalName, InvalidValueReason.InvalidValue);
                } catch (OverflowException) {
                    throw new InvalidXMLValueException("OverlayText", xAttrib.LocalName, InvalidValueReason.Overflow);
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

        public override void UpdateVariables(Dictionary<string, OverlayVariable> variableDictionary)
        {
            text = VariableEvaluator.ReplaceVariables(originalText, variableDictionary);
            textColor = VariableEvaluator.ReplaceVariables(originalColor, variableDictionary);

            if (MyLabel != null)
            {
                MyLabel.Content = text;
                MyLabel.Foreground = GetColorBrush(textColor);
            }
        }

    }

}