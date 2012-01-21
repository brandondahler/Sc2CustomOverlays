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
using System.Collections.ObjectModel;

namespace Sc2CustomOverlays.Code.OverlayVariables
{
    public partial class OverlayDropDown : OverlayVariable
    {
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

        public struct DropDownOption
        {
            public string name;
            public string value;
        }

        
        private ObservableCollection<DropDownOption> options = new ObservableCollection<DropDownOption>();
        public ObservableCollection<DropDownOption> ItemOptions { get { return options; } }
        
        public OverlayDropDown()
        {
            InitializeComponent();
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
                    }
                } catch (FormatException) {
                    throw new InvalidXMLValueException("OverlayDropDown", vNodeAttrib.Value, InvalidXMLValueException.Reason.FormatIncorrect);
                } catch (OverflowException) {
                    throw new InvalidXMLValueException("OverlayDropDown", vNodeAttrib.Value, InvalidXMLValueException.Reason.Overflow);
                }
            }

            XmlNodeList oNodes = vNode.SelectNodes("Option");
            foreach (XmlNode oNode in oNodes)
            {
                string lbl = null;
                string val = null;
                
                foreach (XmlAttribute xAttrib in oNode.Attributes)
                {
                    switch (xAttrib.LocalName)
                    {
                        case "label":
                            lbl = xAttrib.Value;
                            break;

                        case "value":
                            val = xAttrib.Value;
                            break;
                    }
                }

                if (lbl != null && val == null)
                    val = lbl;

                if (lbl == null && val != null)
                    lbl = val;

                if (lbl != null && val != null)
                    options.Add(new DropDownOption() { name = lbl, value = val });
            }


        }

        public override void FromNetwork(string value)
        {
            currentValue = value;
        }

        public override OverlayControlsContainer GetElements()
        {
            OverlayControlsContainer occ = base.GetElements();

            occ.Save.AddHandler(Button.ClickEvent, new RoutedEventHandler(SaveUpdateHandler));
            occ.Reset.AddHandler(Button.ClickEvent, new RoutedEventHandler(ClearUpdateHandler));
           
            return occ;
        }

        private void SaveUpdateHandler(object sender, RoutedEventArgs rea)
        {    
            currentValue = ((DropDownOption) ValueDropDown.SelectedItem).name;
        }

        private void ClearUpdateHandler(object sender, RoutedEventArgs rea)
        {
            currentValue = null;
        }
    }
}
