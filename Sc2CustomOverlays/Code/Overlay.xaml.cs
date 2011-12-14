using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Sc2CustomOverlays.Code.OverlayItems;
using Sc2CustomOverlays.Code.OverlayVariables;
using System.Xml;
using Sc2CustomOverlays.Code.Exceptions;

namespace Sc2CustomOverlays.Code
{
    /// <summary>
    /// Interaction logic for Overlay.xaml
    /// </summary>
    public partial class Overlay : Window
    {
        public bool AllowClose = false;
        public bool AllowDrag = false;

        protected string startDirectory = "/";

        protected Thickness? margin = null;
        protected HorizontalAlignment hAlign = HorizontalAlignment.Center;
        protected VerticalAlignment vAlign = VerticalAlignment.Top;

        private List<OverlayItem> overlayItems = new List<OverlayItem>();
        private Dictionary<string, OverlayVariable> variableDictionary = null;

        public Overlay(string startDir)
        {
            InitializeComponent();

            startDirectory = startDir;
        }

        public List<FrameworkElement> GetOverlayControls()
        {
            List<FrameworkElement> controlList = new List<FrameworkElement>();

            foreach (OverlayItem oi in overlayItems)
            {
                controlList.Add(oi.GetElement());
            }

            return controlList;
        }

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
                                margin = (Thickness)(new ThicknessConverter()).ConvertFromString(xAttrib.Value);
                                break;

                            case "halign":
                                hAlign = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), xAttrib.Value);
                                break;

                            case "valign":
                                vAlign = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), xAttrib.Value);
                                break;

                        }
                    }
                    catch (FormatException)
                    {
                        throw new InvalidXMLValueException("Overlay", xAttrib.LocalName, InvalidValueReason.FormatIncorrect);
                    }
                    catch (ArgumentNullException)
                    {
                        throw new InvalidXMLValueException("Overlay", xAttrib.LocalName, InvalidValueReason.NotSpecified);
                    }
                    catch (ArgumentException)
                    {
                        throw new InvalidXMLValueException("Overlay", xAttrib.LocalName, InvalidValueReason.InvalidValue);
                    }
                    catch (OverflowException)
                    {
                        throw new InvalidXMLValueException("Overlay", xAttrib.LocalName, InvalidValueReason.Overflow);
                    } catch (Exception) {
                        throw new InvalidXMLValueException("Overlay", xAttrib.LocalName, InvalidValueReason.InvalidValue);
                    }
                }

            }
            catch (InvalidXMLValueException ex)
            {
                MessageBox.Show(ex.Message);
                throw new OverlayCreationException(OverlayCreationFailure.InvalidXML);
            }

            foreach (XmlNode xNode in xOverlayNode.ChildNodes)
            {
                OverlayItem oi = null;

                switch (xNode.LocalName)
                {
                    case "Image":
                        oi = new OverlayImage(startDirectory);
                        break;
                    case "Text":
                        oi = new OverlayText();
                        break;
                    case "Gradient":
                        oi = new OverlayGradient();
                        break;
                }

                if (oi != null)
                {
                    try
                    {
                        oi.FromXML(xNode);
                    }
                    catch (InvalidXMLValueException ex)
                    {
                        MessageBox.Show(ex.Message);
                        throw new OverlayCreationException(OverlayCreationFailure.InvalidXML);
                    }
                    overlayItems.Add(oi);
                }
            }
        }

        public void SetVariableDictionary(Dictionary<string, OverlayVariable> vDict)
        {
            variableDictionary = vDict;
            UpdateVariables();
        }


        public void UpdateVariables()
        {
            foreach (OverlayItem oi in overlayItems)
            {
                oi.UpdateVariables(variableDictionary);
            }
        }

        public void ReloadOverlayControls()
        {
            overlayGrid.Children.Clear();

            foreach (FrameworkElement fe in GetOverlayControls())
                overlayGrid.Children.Add(fe);

            
        }

        private void RepositionWindow()
        {
            double widthAdjust = 0.0;
            double heightAdjust = 0.0;

            switch (hAlign)
            {
                case HorizontalAlignment.Center:
                    widthAdjust = SystemParameters.PrimaryScreenWidth / 2 - ActualWidth / 2;
                    break;

                case HorizontalAlignment.Right:
                    widthAdjust = SystemParameters.PrimaryScreenWidth - ActualWidth;
                    break;
            }

            switch (vAlign)
            {
                case VerticalAlignment.Center:
                    heightAdjust = SystemParameters.PrimaryScreenHeight / 2 - ActualHeight / 2;
                    break;

                case VerticalAlignment.Bottom:
                    heightAdjust = SystemParameters.PrimaryScreenHeight - ActualHeight;
                    break;
            }

            if (margin.HasValue)
            {
                Top = heightAdjust + margin.Value.Top - margin.Value.Bottom;
                Left = widthAdjust + margin.Value.Left - margin.Value.Right;
            } else {
                Top = heightAdjust;
                Left = widthAdjust;
            }
            
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (AllowDrag)
                DragMove();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!AllowClose)
                e.Cancel = true;
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
                ReloadOverlayControls();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            RepositionWindow();
        }

    }
}
