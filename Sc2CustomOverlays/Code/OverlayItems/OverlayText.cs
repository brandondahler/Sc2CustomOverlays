using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using System.Xml;
using System.Text.RegularExpressions;


namespace Sc2CustomOverlays
{
    class OverlayText : OverlayItem
    {
        private string originalText = "";
        private string text = "";
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
                VerticalAlignment = (vAlign.HasValue ? vAlign.Value : VerticalAlignment.Top)
            };
            
            if (hAlign.HasValue)
                MyLabel.HorizontalContentAlignment = hAlign.Value;

            if (vAlign.HasValue)
                MyLabel.VerticalContentAlignment = vAlign.Value;

            if (margin.HasValue) MyLabel.Margin = margin.Value;
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
                switch (xAttrib.LocalName)
                {
                    case "value":
                        originalText = xAttrib.Value;
                        text = originalText;
                        break;
                    case "color":
                        originalColor = xAttrib.Value;
                        textColor = originalColor;
                        break;
                    case "size":
                        fontSize = int.Parse(xAttrib.Value);
                        break;
                    case "family":
                        fontFamily = new FontFamily(xAttrib.Value);
                        break;
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