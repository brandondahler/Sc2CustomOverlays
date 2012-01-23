using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;

using Sc2CustomOverlays.Models.Exceptions;
using Sc2CustomOverlays.Models;
using System.IO;
using Sc2CustomOverlays.ViewModel.OverlayVariables;

namespace Sc2CustomOverlays.ViewModel.OverlayItems
{
    class OverlayImageViewModel : OverlayItemBaseModel
    {
        

        // Properties
        #region ImageLocation
            private string _imageLocation = "";
            public string ImageLocation
            {
                get { return _imageLocation; }
                set
                {
                    _imageLocation = value;
                    RaisePropertyChanged("ImageLocation");
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

        #region ImageStretch
            public Stretch ImageStretch
            {
                get
                {
                    if (!double.IsNaN(Height) || !double.IsNaN(Width))
                    {
                        if (!double.IsNaN(Height) && !double.IsNaN(Width))
                            return Stretch.Fill;

                        return Stretch.Uniform;
                    }

                    return Stretch.None;
                }
            }
        #endregion

        private string originalImageLocation = "";

        public override void FromXML(XmlNode xImageNode)
        {
            base.FromXML(xImageNode);

            try
            {
                originalImageLocation = xImageNode.Attributes.GetNamedItem("location").Value;
            } catch (Exception) {
                throw new InvalidXMLValueException("OverlayImage", "location", InvalidXMLValueException.Reason.NotSpecified);
            }

            foreach (XmlAttribute xAttrib in xImageNode.Attributes)
            {
                try
                {
                    switch (xAttrib.LocalName)
                    {
                        case "height":
                            Height = double.Parse(xAttrib.Value);
                            break;
                        case "width":
                            Width = double.Parse(xAttrib.Value);
                            break;
                    }
                } catch (FormatException) {
                    throw new InvalidXMLValueException("OverlayImage", xAttrib.LocalName, InvalidXMLValueException.Reason.FormatIncorrect);
                } catch (ArgumentNullException) {
                    throw new InvalidXMLValueException("OverlayImage", xAttrib.LocalName, InvalidXMLValueException.Reason.NotSpecified);
                } catch (OverflowException) {
                    throw new InvalidXMLValueException("OverlayImage", xAttrib.LocalName, InvalidXMLValueException.Reason.Overflow);
                }
            }
        }

        public override void UpdateVariables(Dictionary<string, OverlayVariableBaseModel> variableDictionary)
        {
            ImageLocation = Path.Combine(OverlaySettings.Instance.Location.Directory.FullName, 
                                        VariableEvaluator.ReplaceVariables(originalImageLocation, variableDictionary));
        }
    }

}