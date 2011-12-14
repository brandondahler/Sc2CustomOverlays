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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using Sc2CustomOverlays.Code.Exceptions;
using System.Windows.Controls.Primitives;

namespace Sc2CustomOverlays.Code.OverlayVariables
{
    public partial class OverlayItemSelector : OverlayVariable
    {
        public string startDirectory { get; set; }

        public override string Value
        {
            get
            {
                if (currentValue == null)
                    return defaultValue;

                return currentValue;
            }
        }


        private string _currentValue = null;
        protected string currentValue
        {
            get { return _currentValue; }
            set
            {
                _currentValue = value;
                RaiseUpdated();
            }
        }
        protected string defaultValue = null;
        protected int? columns = null;
        protected double? itemHeight = null;
        protected double? itemWidth = null;


        protected enum OptionTypes
        {
            None = 0,
            Text,
            Color,
            Image
        }

        protected struct ItemSelectorOption
        {
            public object label { get; set; }
            public OptionTypes type { get; set; }
            public string value { get; set; }
            public string alt { get; set; }
        }

        private List<ItemSelectorOption> options = new List<ItemSelectorOption>();

        public OverlayItemSelector(string startDir)
            : base()
        {
            InitializeComponent();
            startDirectory = startDir;

        }

        public override void FromXML(XmlNode vNode)
        {
            base.FromXML(vNode);

            foreach (XmlAttribute vNodeAttrib in vNode.Attributes)
            {
                try
                {
                    switch (vNodeAttrib.LocalName)
                    {
                        case "default":
                            defaultValue = vNodeAttrib.Value;
                            break;
                        case "value":
                            currentValue = vNodeAttrib.Value;
                            break;
                        case "columns":
                            columns = int.Parse(vNodeAttrib.Value);
                            break;
                        case "itemHeight":
                            itemHeight = double.Parse(vNodeAttrib.Value);
                            break;
                        case "itemWidth":
                            itemWidth = double.Parse(vNodeAttrib.Value);
                            break;
                    }
                } catch (FormatException) {
                    throw new InvalidXMLValueException("OverlayItemSelector", vNodeAttrib.Value, InvalidValueReason.FormatIncorrect);
                } catch (ArgumentNullException) {
                    throw new InvalidXMLValueException("OverlayItemSelector", vNodeAttrib.Value, InvalidValueReason.NotSpecified);
                } catch (OverflowException) {
                    throw new InvalidXMLValueException("OverlayItemSelector", vNodeAttrib.Value, InvalidValueReason.Overflow);
                }
            }

            XmlNodeList oNodes = vNode.SelectNodes("Option");
            foreach (XmlNode oNode in oNodes)
            {
                ItemSelectorOption option = new ItemSelectorOption();

                foreach (XmlAttribute xAttrib in oNode.Attributes)
                {
                    try
                    {
                        switch (xAttrib.LocalName)
                        {
                            case "text":
                                option.label = xAttrib.Value;
                                option.type = OptionTypes.Text;
                                break;

                            case "color":
                                option.label = xAttrib.Value;
                                option.type = OptionTypes.Color;

                                if (ColorConverter.ConvertFromString(xAttrib.Value) == null)
                                    throw new InvalidXMLValueException("OverlayItemSelector:Option", xAttrib.LocalName, InvalidValueReason.InvalidValue);

                                break;

                            case "image":
                                option.label = new BitmapImage(new Uri("pack://siteoforigin:,,,/" + startDirectory + xAttrib.Value));
                                option.type = OptionTypes.Image;

                                if (option.label == null)
                                    throw new InvalidXMLValueException("OverlayItemSelector:Option", xAttrib.LocalName, InvalidValueReason.InvalidValue);

                                break;

                            case "value":
                                option.value = xAttrib.Value;
                                break;

                            case "alt":
                                option.alt = xAttrib.Value;
                                break;
                        }
                    } catch (ArgumentNullException) {
                        throw new InvalidXMLValueException("OverlayItemSelector:Option", xAttrib.LocalName, InvalidValueReason.NotSpecified);
                    }
                }

                options.Add(option);
            }


        }

        public override OverlayControlsContainer GetElements()
        {
            OverlayControlsContainer occ = base.GetElements();

            if (columns.HasValue)
                ValueGrid.Columns = columns.Value;

            foreach (ItemSelectorOption option in options)
            {
                RadioButton optionButton = new RadioButton()
                {
                    Margin = new Thickness(1),
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    VerticalContentAlignment = VerticalAlignment.Stretch,
                    Content = option
                };

                if (itemHeight.HasValue)
                    optionButton.Height = itemHeight.Value;
                
                if (itemWidth.HasValue)
                    optionButton.Width = itemWidth.Value;

                optionButton.AddHandler(ToggleButton.ClickEvent, new RoutedEventHandler(OptionChangedHandler));
                optionButton.CommandParameter = option.value;

                ValueGrid.Children.Add(optionButton);
            }

            if (columns.HasValue && ValueGrid.Children.Count < columns.Value)
                ValueGrid.Columns = ValueGrid.Children.Count;

            if (currentValue == null)
                currentValue = options.First().value;

            occ.save = null;

            occ.reset.AddHandler(Button.ClickEvent, new RoutedEventHandler(ClearUpdateHandler));

            return occ;
        }

        private void UpdateHandler()
        {
            foreach (RadioButton rb in ValueGrid.Children)
            {
                rb.IsChecked = ( ((ItemSelectorOption) rb.Content).value == Value);
            }
        }

        private void OptionChangedHandler(object sender, RoutedEventArgs rea)
        {
            ToggleButton option = (ToggleButton)sender;
            currentValue = (string)option.CommandParameter;
        }

        private void ClearUpdateHandler(object sender, RoutedEventArgs rea)
        {
            currentValue = null;
        }
    }
}
