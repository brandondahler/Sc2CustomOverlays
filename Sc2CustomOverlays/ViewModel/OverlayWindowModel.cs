// -----------------------------------------------------------------------
// <copyright file="OverlayWindowModel.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Sc2CustomOverlays.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Sc2CustomOverlays.Models.MVVMHelpers;
    using System.Windows;
    using System.Collections.ObjectModel;
    using Sc2CustomOverlays.ViewModel.OverlayItems;
    using System.Xml;
    using Sc2CustomOverlays.Models.Exceptions;
    using System.Windows.Media;
    using Sc2CustomOverlays.ViewModel.OverlayVariables;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class OverlayWindowModel : ObservableModel
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
        #region HorizontalAlign
            private HorizontalAlignment _horizontalAlign = HorizontalAlignment.Center;
            public HorizontalAlignment HorizontalAlign
            {
                get { return _horizontalAlign; }
                set
                {
                    _horizontalAlign = value;
                    RaisePropertyChanged("HorizontalAlign");
                    RaisePropertyChanged("Left");
                }
            }
        #endregion
        #region VerticalAlign
            protected VerticalAlignment _verticalAlign = VerticalAlignment.Top;
            public VerticalAlignment VerticalAlign
            {
                get { return _verticalAlign; }
                set
                {
                    _verticalAlign = value;
                    RaisePropertyChanged("VerticalAlign");
                    RaisePropertyChanged("Top");
                }
            }
        #endregion

        #region ActualHeight
            private double _actualHeight;
            public double ActualHeight 
            { 
                get { return _actualHeight;}
                set
                {
                    _actualHeight = value;
                    RaisePropertyChanged("ActualHeight");
                    RaisePropertyChanged("Top");
                }
            }
        #endregion
        #region ActualWidth
            private double _actualWidth;
            public double ActualWidth 
            { 
                get { return _actualWidth; }
                set
                {
                    _actualWidth = value;
                    RaisePropertyChanged("ActualWidth");
                    RaisePropertyChanged("Left");
                }
            }
        #endregion

        #region Top
            public double Top
            {
                get 
                {
                    double heightAdjust = 0.0;
                    switch (VerticalAlign)
                    {
                        case VerticalAlignment.Center:
                            heightAdjust = SystemParameters.PrimaryScreenHeight / 2 - ActualHeight / 2;
                            break;

                        case VerticalAlignment.Bottom:
                            heightAdjust = SystemParameters.PrimaryScreenHeight - ActualHeight;
                            break;
                    }

                    return heightAdjust + Margin.Top - Margin.Bottom;
                }
            }
        #endregion
        #region Left
            public double Left
            {
                get
                {
                    double widthAdjust = 0.0;
                    switch (HorizontalAlign)
                    {
                        case HorizontalAlignment.Center:
                            widthAdjust = SystemParameters.PrimaryScreenWidth / 2 - ActualWidth / 2;
                            break;

                        case HorizontalAlignment.Right:
                            widthAdjust = SystemParameters.PrimaryScreenWidth - ActualWidth;
                            break;
                    }
                    
                    return widthAdjust + Margin.Left - Margin.Right;
                }
            }
        #endregion
        
        #region Background
            private Brush _background = new SolidColorBrush(Colors.Transparent);
            public Brush Background
            {
                get { return _background; }
                set
                {
                    _background = value;
                    RaisePropertyChanged("Background");
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
                    RaisePropertyChanged("Left");
                    RaisePropertyChanged("SizeWindowToContent");
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
                    RaisePropertyChanged("Top");
                    RaisePropertyChanged("SizeWindowToContent");
                }
            }
        #endregion
        #region SizeWindowToContent
            public SizeToContent SizeWindowToContent
            {
                get
                {
                    if (!double.IsNaN(Width) || !double.IsNaN(Height))
                    {
                        if (!double.IsNaN(Width) && !double.IsNaN(Height))
                        {
                            return SizeToContent.Manual;
                        } else {
                            if (double.IsNaN(Width) && !double.IsNaN(Height))
                                return SizeToContent.Width;
                            else
                                return SizeToContent.Height;
                        }
                    } else {
                        return SizeToContent.WidthAndHeight;
                    }
                }
            }
        #endregion


        #region OverlayItems
            private ObservableCollection<OverlayItemBaseModel> _overlayItems = new ObservableCollection<OverlayItemBaseModel>();
            public ObservableCollection<OverlayItemBaseModel> OverlayItems { get { return _overlayItems; } }
        #endregion

        public void FromXML(XmlNode xOverlayNode)
        {
            try
            {
                foreach (XmlAttribute xAttrib in xOverlayNode.Attributes)
                {
                    try
                    {
                        switch (xAttrib.LocalName)
                        {
                            case "margin":
                                Margin = (Thickness) (new ThicknessConverter()).ConvertFromString(xAttrib.Value);
                                break;

                            case "halign":
                                HorizontalAlign = (HorizontalAlignment) Enum.Parse(typeof(HorizontalAlignment), xAttrib.Value);
                                break;

                            case "valign":
                                VerticalAlign = (VerticalAlignment) Enum.Parse(typeof(VerticalAlignment), xAttrib.Value);
                                break;

                            case "background":
                                Background = new SolidColorBrush((Color) ColorConverter.ConvertFromString(xAttrib.Value));
                                break;

                            case "width":
                                Width = double.Parse(xAttrib.Value);
                                break;

                            case "height":
                                Height = double.Parse(xAttrib.Value);
                                break;

                        }
                    } catch (FormatException) {
                        throw new InvalidXMLValueException("Overlay", xAttrib.LocalName, InvalidXMLValueException.Reason.FormatIncorrect);
                    } catch (ArgumentNullException) {
                        throw new InvalidXMLValueException("Overlay", xAttrib.LocalName, InvalidXMLValueException.Reason.NotSpecified);
                    } catch (ArgumentException) {
                        throw new InvalidXMLValueException("Overlay", xAttrib.LocalName, InvalidXMLValueException.Reason.InvalidValue);
                    } catch (OverflowException) {
                        throw new InvalidXMLValueException("Overlay", xAttrib.LocalName, InvalidXMLValueException.Reason.Overflow);
                    } catch (Exception) {
                        throw new InvalidXMLValueException("Overlay", xAttrib.LocalName, InvalidXMLValueException.Reason.InvalidValue);
                    }
                }

            } catch (InvalidXMLValueException ex) {
                MessageBox.Show(ex.Message);
                throw new OverlayCreationException(OverlayCreationException.Reason.InvalidXML);
            }

            foreach (XmlNode xNode in xOverlayNode.ChildNodes)
            {
                OverlayItemBaseModel oi = null;

                switch (xNode.LocalName)
                {
                    case "Image":
                        oi = new OverlayImageViewModel();
                        break;
                    case "Text":
                        oi = new OverlayTextViewModel();
                        break;
                    case "Gradient":
                        oi = new OverlayGradientViewModel();
                        break;
                }

                if (oi != null)
                {
                    try
                    {
                        oi.FromXML(xNode);
                    } catch (InvalidXMLValueException ex) {
                        MessageBox.Show(ex.Message);
                        throw new OverlayCreationException(OverlayCreationException.Reason.InvalidXML);
                    }
                    OverlayItems.Add(oi);
                }
            }
        }

        public void UpdateVariables(Dictionary<string, OverlayVariableBaseModel> variableDictionary)
        {
            foreach (OverlayItemBaseModel oi in OverlayItems)
                oi.UpdateVariables(variableDictionary);
        }


    }
}
