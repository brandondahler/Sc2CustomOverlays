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
using System.Collections.ObjectModel;
using System.ComponentModel;

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

        private string _defaultValue = null;
        protected string defaultValue
        {
            get { return _defaultValue; }
            set
            {
                _defaultValue = value;
                RaiseUpdated();
            }
        }


        protected int? columns = null;
        protected double? itemHeight = null;
        protected double? itemWidth = null;


        public enum OptionTypes
        {
            None = 0,
            Text,
            Color,
            Image
        }

        public class ItemSelectorOption : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public object label { get; set; }
            public OptionTypes type { get; set; }
            public string value { get; set; }
            public string alt { get; set; }
            public bool IsChecked { get; set; }

            public void RaisePropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private ObservableCollection<ItemSelectorOption> options = new ObservableCollection<ItemSelectorOption>();
        public ObservableCollection<ItemSelectorOption> ItemOptions { get { return options; } }
        
        public bool ItemHeightHasValue { get { return itemHeight.HasValue; } }
        public double ItemHeightValue { get { return itemHeight.Value; } }

        public bool ItemWidthHasValue { get { return itemWidth.HasValue; } }
        public double ItemWidthValue { get { return itemWidth.Value; } }

        public int GridColumnsValue 
        { 
            get 
            {
                if (columns.HasValue)
                {
                    if (options.Count < columns.Value)
                        return options.Count;
                    else
                        return columns.Value;
                }

                return 0; 
            } 
        }

        public OverlayItemSelector(string startDir)
            : base()
        {
            InitializeComponent();
            startDirectory = startDir;
            this.VariableUpdated += new VariableUpdatedHandler(SelectionUpdated);
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
                    throw new InvalidXMLValueException("OverlayItemSelector", vNodeAttrib.Value, InvalidXMLValueException.Reason.FormatIncorrect);
                } catch (ArgumentNullException) {
                    throw new InvalidXMLValueException("OverlayItemSelector", vNodeAttrib.Value, InvalidXMLValueException.Reason.NotSpecified);
                } catch (OverflowException) {
                    throw new InvalidXMLValueException("OverlayItemSelector", vNodeAttrib.Value, InvalidXMLValueException.Reason.Overflow);
                }
            }

            XmlNodeList oNodes = vNode.SelectNodes("Option");
            bool isFirst = true;

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
                                    throw new InvalidXMLValueException("OverlayItemSelector:Option", xAttrib.LocalName, InvalidXMLValueException.Reason.InvalidValue);

                                break;

                            case "image":
                                option.label = new BitmapImage(new Uri(startDirectory + xAttrib.Value));
                                option.type = OptionTypes.Image;

                                if (option.label == null)
                                    throw new InvalidXMLValueException("OverlayItemSelector:Option", xAttrib.LocalName, InvalidXMLValueException.Reason.InvalidValue);

                                break;

                            case "value":
                                option.value = xAttrib.Value;
                                break;

                            case "alt":
                                option.alt = xAttrib.Value;
                                break;
                        }
                    } catch (ArgumentNullException) {
                        throw new InvalidXMLValueException("OverlayItemSelector:Option", xAttrib.LocalName, InvalidXMLValueException.Reason.NotSpecified);
                    }
                }

                option.IsChecked = isFirst;
                options.Add(option);

                isFirst = false;
            }


        }

        public override void FromNetwork(string value)
        {
            currentValue = value;
        }

        public override OverlayControlsContainer GetElements()
        {
            OverlayControlsContainer occ = base.GetElements();

            if (currentValue == null)
                currentValue = options.First().value;

            occ.Save = null;

            occ.Reset.AddHandler(Button.ClickEvent, new RoutedEventHandler(ClearUpdateHandler));

            return occ;
        }

        private void OptionChangedHandler(object sender, RoutedEventArgs rea)
        {
            ToggleButton option = (ToggleButton)sender;
            currentValue = (string)option.CommandParameter;
        }

        private void SelectionUpdated(OverlayVariable ov)
        {
            foreach (ItemSelectorOption iso in options)
            {
                if (iso.value == Value)
                    iso.IsChecked = true;
                else
                    iso.IsChecked = false;

                iso.RaisePropertyChanged("IsChecked");
            }
        }

        private void ClearUpdateHandler(object sender, RoutedEventArgs rea)
        {
            currentValue = null;
        }
    }
}
