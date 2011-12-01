using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sc2CustomOverlays
{
    class OverlayGradient : OverlayItem
    {
        protected struct GradientColor
        {
            public string color;
            public double? offset;
            public bool? transparent;
        }


        protected double? height = null;
        protected double? width = null;
        protected double? angle = null;
        protected List<GradientColor> originalGradientStops = new List<GradientColor>();
        protected GradientStopCollection gradientStops = new GradientStopCollection();

        Rectangle MyRectangle = null;

        
        public override FrameworkElement GetElement()
        {
            MyRectangle = new Rectangle()
            {
                HorizontalAlignment = (hAlign.HasValue ? hAlign.Value : HorizontalAlignment.Stretch),
                VerticalAlignment = (vAlign.HasValue ? vAlign.Value : VerticalAlignment.Stretch)
            };

            UpdateGradient();

            if (margin.HasValue)
                MyRectangle.Margin = margin.Value;

            return MyRectangle;
        }

        public override void FromXML(XmlNode xGradientNode)
        {
            base.FromXML(xGradientNode);

            originalGradientStops = new List<GradientColor>();
            XmlNodeList xColors = xGradientNode.SelectNodes("Color");
            foreach (XmlNode xColor in xColors)
            {
                GradientColor gc = new GradientColor();

                gc.color = xColor.Attributes.GetNamedItem("color").Value;
                gc.offset = double.Parse(xColor.Attributes.GetNamedItem("offset").Value);
                gc.transparent = null;

                foreach (XmlAttribute xAttrib in xColor.Attributes)
                {
                    switch (xAttrib.LocalName)
                    {
                        case "transparent":
                            gc.transparent = bool.Parse(xAttrib.Value);
                            break;
                    }
                }

                originalGradientStops.Add(gc);
            }

            foreach (XmlAttribute xAttrib in xGradientNode.Attributes)
            {
                switch (xAttrib.LocalName)
                {
                    case "angle":
                        angle = double.Parse(xAttrib.Value); 
                        break;
                    case "height":
                        height = double.Parse(xAttrib.Value);
                        break;
                    case "width":
                        width = double.Parse(xAttrib.Value);
                        break;
                    
                }
            }

        }

        private void UpdateGradient()
        {
            if (MyRectangle != null)
            {
                LinearGradientBrush lgb = null;

                if (angle.HasValue)
                    lgb = new LinearGradientBrush(new GradientStopCollection(), angle.Value);
                else
                    lgb = new LinearGradientBrush(new GradientStopCollection(), 0.0);
                
                lgb.GradientStops = gradientStops;
                MyRectangle.Fill = lgb;

                if (height.HasValue)
                    MyRectangle.Height = height.Value;

                if (width.HasValue)
                    MyRectangle.Width = width.Value;

            }
        }

        public override void UpdateVariables(Dictionary<string, OverlayVariable> variableDictionary)
        {
            gradientStops = new GradientStopCollection();
            foreach (GradientColor gsPair in originalGradientStops)
            {
                GradientStop gs = new GradientStop();
                Color gradColor = (Color) ColorConverter.ConvertFromString(VariableEvaluator.ReplaceVariables(gsPair.color, variableDictionary));

                if (gsPair.transparent.HasValue && gsPair.transparent.Value)
                    gradColor.A = 0;

                gs.Color = gradColor;

                if (gsPair.offset.HasValue)
                    gs.Offset = gsPair.offset.Value;

                gradientStops.Add(gs);
            }

            UpdateGradient();
        }
    }
}
