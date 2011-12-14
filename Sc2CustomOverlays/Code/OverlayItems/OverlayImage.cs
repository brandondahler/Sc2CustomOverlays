using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;

using Sc2CustomOverlays.Code.OverlayVariables;
using Sc2CustomOverlays.Code.Exceptions;

namespace Sc2CustomOverlays.Code.OverlayItems
{
    class OverlayImage : OverlayItem
    {
        protected string startDirectory = "/";

        protected string originalImageLocation = "";
        protected string imageLocation = "";
        protected int? height = null;
        protected int? width = null;

        Image MyImage = null;

        public OverlayImage(string startDir) : base()
        {
            startDirectory = startDir;
        }

        public override FrameworkElement GetElement()
        {
            MyImage = new Image()
            {
                HorizontalAlignment = (hAlign.HasValue ? hAlign.Value : HorizontalAlignment.Left),
                VerticalAlignment = (vAlign.HasValue ? vAlign.Value : VerticalAlignment.Top)
            };

            UpdateImage();

            if (margin.HasValue)
                MyImage.Margin = margin.Value;

            return MyImage;
        }

        public override void FromXML(XmlNode xImageNode)
        {
            base.FromXML(xImageNode);

            try
            {
                originalImageLocation = xImageNode.Attributes.GetNamedItem("location").Value;
                imageLocation = originalImageLocation;
            } catch (Exception) {
                throw new InvalidXMLValueException("OverlayImage", "location", InvalidValueReason.NotSpecified);
            }

            foreach (XmlAttribute xAttrib in xImageNode.Attributes)
            {
                try
                {
                    switch (xAttrib.LocalName)
                    {
                        case "height":
                            height = int.Parse(xAttrib.Value);
                            break;
                        case "width":
                            width = int.Parse(xAttrib.Value);
                            break;
                    }
                } catch (FormatException) {
                    throw new InvalidXMLValueException("OverlayImage", xAttrib.LocalName, InvalidValueReason.FormatIncorrect);
                } catch (ArgumentNullException) {
                    throw new InvalidXMLValueException("OverlayImage", xAttrib.LocalName, InvalidValueReason.NotSpecified);
                } catch (OverflowException) {
                    throw new InvalidXMLValueException("OverlayImage", xAttrib.LocalName, InvalidValueReason.Overflow);
                }
            }
        }

        private void UpdateImage()
        {
            if (MyImage != null)
            {
                try
                {
                    BitmapImage bi = new BitmapImage(new Uri("pack://siteoforigin:,,,/" + startDirectory + imageLocation));
                    MyImage.Source = bi;

                    if (height.HasValue)
                        MyImage.Height = height.Value;
                    else
                        MyImage.Height = bi.PixelHeight;

                    if (width.HasValue)
                        MyImage.Width = width.Value;
                    else
                        MyImage.Width = bi.PixelWidth;

                } catch { 

                }
            }
        }

        public override void UpdateVariables(Dictionary<string, OverlayVariable> variableDictionary)
        {
            imageLocation = VariableEvaluator.ReplaceVariables(originalImageLocation, variableDictionary);
            UpdateImage();
        }
    }

}