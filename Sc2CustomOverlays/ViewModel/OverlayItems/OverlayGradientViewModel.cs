using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;

using Sc2CustomOverlays.Models.Exceptions;
using Sc2CustomOverlays.Models;
using System.Collections.ObjectModel;
using Sc2CustomOverlays.ViewModel.OverlayVariables;

namespace Sc2CustomOverlays.ViewModel.OverlayItems
{
    class OverlayGradientViewModel : OverlayItemBaseModel
    {
        protected override HorizontalAlignment HorizontalAlignDefault { get { return HorizontalAlignment.Stretch; } }
        protected override VerticalAlignment VerticalAlignDefault { get { return VerticalAlignment.Stretch; } }

        // Properties
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
        #region Height
            private double _height = double.NaN;
            public double Height
            {
                get { return _height; }
                set
                {
                    _height = value;
                    RaisePropertyChanged("Height");
                }
            }
        #endregion

        #region GradientBrush
            private LinearGradientBrush _gradientBrush = null;
            public LinearGradientBrush GradientBrush
            {
                get { return _gradientBrush; }
                set
                {
                    _gradientBrush = value;
                    RaisePropertyChanged("GradientBrush");
                }
            }
        #endregion

        private struct GradientColor
        {
            public string color;
            public double? offset;
            public bool? transparent;
        }

        private List<GradientColor> originalGradientStops = new List<GradientColor>();
        private double angle = 0.0;

        #region GradientStart
            private Point GradientStart
            {
                get
                {
                    if (angle < 45)
                        return new Point(0.0, 0.5 - Math.Sin(angle * Math.PI / 180));

                    return new Point(0.5 - Math.Cos(angle * Math.PI / 180), 0);
                }
            }
        #endregion
        #region GradientEnd
            private Point GradientEnd
            {
                get
                {
                    if (angle < 45)
                        return new Point(1.0, 0.5 + Math.Sin(angle * Math.PI / 180));

                    return new Point(0.5 + Math.Cos(angle * Math.PI / 180), 1.0);
                }
            }
        #endregion

        public override void FromXML(XmlNode xGradientNode)
        {
            base.FromXML(xGradientNode);

            originalGradientStops = new List<GradientColor>();
            XmlNodeList xColors = xGradientNode.SelectNodes("Color");
            foreach (XmlNode xColor in xColors)
            {
                GradientColor gc = new GradientColor();

                try
                {
                    gc.color = xColor.Attributes.GetNamedItem("color").Value;
                    gc.offset = double.Parse(xColor.Attributes.GetNamedItem("offset").Value);
                } catch (FormatException) {
                    throw new InvalidXMLValueException("OverlayGradient:Color", "offset", InvalidXMLValueException.Reason.FormatIncorrect);
                } catch (ArgumentNullException) {
                    throw new InvalidXMLValueException("OverlayGradient:Color", "offset", InvalidXMLValueException.Reason.NotSpecified);
                } catch (OverflowException) {
                    throw new InvalidXMLValueException("OverlayGradient:Color", "offset", InvalidXMLValueException.Reason.Overflow);
                } catch (Exception) {
                    throw new InvalidXMLValueException("OverlayGradient:Color", "color or offset", InvalidXMLValueException.Reason.NotSpecified);
                }

                gc.transparent = null;

                foreach (XmlAttribute xAttrib in xColor.Attributes)
                {
                    try
                    {
                        switch (xAttrib.LocalName)
                        {
                            case "transparent":
                                gc.transparent = bool.Parse(xAttrib.Value);
                                break;
                        }
                    } catch (FormatException) {
                        throw new InvalidXMLValueException("OverlayGradient:Color", "transparent", InvalidXMLValueException.Reason.FormatIncorrect);
                    } catch (ArgumentNullException) {
                        throw new InvalidXMLValueException("OverlayGradient:Color", "transparent", InvalidXMLValueException.Reason.NotSpecified);
                    }
                }

                originalGradientStops.Add(gc);
            }

            foreach (XmlAttribute xAttrib in xGradientNode.Attributes)
            {
                try
                {
                    switch (xAttrib.LocalName)
                    {
                        case "angle":
                            angle = double.Parse(xAttrib.Value);
                            break;
                        case "height":
                            Height = double.Parse(xAttrib.Value);
                            break;
                        case "width":
                            Width = double.Parse(xAttrib.Value);
                            break;

                    }
                } catch (FormatException) {
                    throw new InvalidXMLValueException("OverlayGradient", xAttrib.LocalName, InvalidXMLValueException.Reason.FormatIncorrect);
                } catch (ArgumentNullException) {
                    throw new InvalidXMLValueException("OverlayGradient", xAttrib.LocalName, InvalidXMLValueException.Reason.NotSpecified);
                } catch (OverflowException) {
                    throw new InvalidXMLValueException("OverlayGradient", xAttrib.LocalName, InvalidXMLValueException.Reason.Overflow);
                }
            }

        }

        public override void UpdateVariables(Dictionary<string, OverlayVariableBaseModel> variableDictionary)
        {
            LinearGradientBrush lgb = new LinearGradientBrush();
            foreach (GradientColor gsPair in originalGradientStops)
            {
                GradientStop gs = new GradientStop();
                Color gradColor = (Color) ColorConverter.ConvertFromString(VariableEvaluator.ReplaceVariables(gsPair.color, variableDictionary));

                if (gsPair.transparent.HasValue && gsPair.transparent.Value)
                    gradColor.A = 0;
                
                gs.Color = gradColor;

                if (gsPair.offset.HasValue)
                    gs.Offset = gsPair.offset.Value;

                lgb.GradientStops.Add(gs);
            }
            lgb.StartPoint = GradientStart;
            lgb.EndPoint = GradientEnd;

            GradientBrush = lgb;
        }
    }
}
