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

namespace Sc2CustomOverlays.Code.OverlayVariables
{
    public partial class OverlayDropDown : OverlayVariable
    {
        public override string Value
        {
            get
            {
                if (_currentValue != null)
                    return _currentValue;

                return options.ElementAt(defaultIndex).Value;
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
        private int defaultIndex = 0;

        private Dictionary<string, string> options = new Dictionary<string,string>();
        
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
                            defaultIndex = int.Parse(vNodeAttrib.Value);
                            break;
                    }
                } catch (FormatException) {
                    throw new InvalidXMLValueException("OverlayDropDown", vNodeAttrib.Value, InvalidValueReason.FormatIncorrect);
                } catch (OverflowException) {
                    throw new InvalidXMLValueException("OverlayDropDown", vNodeAttrib.Value, InvalidValueReason.Overflow);
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
                    options.Add(lbl, val);
            }


        }

        public override OverlayControlsContainer GetElements()
        {
            OverlayControlsContainer occ = base.GetElements();

            foreach (string optionName in options.Keys)
                ValueDropDown.Items.Add(optionName);

            UpdateSelection();

            occ.save.AddHandler(Button.ClickEvent, new RoutedEventHandler(SaveUpdateHandler));
            occ.reset.AddHandler(Button.ClickEvent, new RoutedEventHandler(ClearUpdateHandler));
           
            return occ;
        }

        private void UpdateSelection()
        {
            if (currentValue != null)
                ValueDropDown.SelectedItem = currentValue;
            else
                ValueDropDown.SelectedIndex = defaultIndex;
        }

        private void SaveUpdateHandler(object sender, RoutedEventArgs rea)
        {    
            currentValue = options[(string)ValueDropDown.SelectedItem];
        }

        private void ClearUpdateHandler(object sender, RoutedEventArgs rea)
        {
            currentValue = null;
        }
    }
}
