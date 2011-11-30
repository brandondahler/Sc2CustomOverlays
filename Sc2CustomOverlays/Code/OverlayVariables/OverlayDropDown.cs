using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace Sc2CustomOverlays
{
    public class OverlayDropDown : OverlayVariable
    {
        public override string Value
        {
            get
            {
                if (value != null)
                    return value;

                return options.ElementAt(defaultIndex).Value;
            }
        }

        private int defaultIndex = 0;

        private string _value = null;
        private string value
        {
            get
            {
                return _value;
            }

            set
            {
                _value = value;
                RaiseUpdated();
            }
        }

        private Dictionary<string, string> options = new Dictionary<string,string>();

        private ComboBox MyDropDown = null;

        public override void FromXML(XmlNode vNode)
        {
            base.FromXML(vNode);

            foreach (XmlAttribute vNodeAttrib in vNode.Attributes)
            {
                switch (vNodeAttrib.LocalName)
                {
                    case "default":
                        defaultIndex = int.Parse(vNodeAttrib.Value);
                        break;
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
            //StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 15) };

            MyDropDown = new ComboBox() { Width = 150 };
            foreach (string optionName in options.Keys)
                MyDropDown.Items.Add(optionName);

            UpdateSelection();

            occ.modifier = MyDropDown;

            occ.save.AddHandler(Button.ClickEvent, new RoutedEventHandler(SaveUpdateHandler));
            occ.reset.AddHandler(Button.ClickEvent, new RoutedEventHandler(ClearUpdateHandler));
           
            return occ;
        }

        private void UpdateSelection()
        {
            if (MyDropDown != null)
            {
                if (value != null)
                    MyDropDown.SelectedItem = value;
                else
                    MyDropDown.SelectedIndex = defaultIndex;
            }
        }

        private void SaveUpdateHandler(object sender, RoutedEventArgs rea)
        {
            if (MyDropDown != null)
            {
                value = options[(string) MyDropDown.SelectedItem];
            }
        }

        private void ClearUpdateHandler(object sender, RoutedEventArgs rea)
        {
            value = null;
            UpdateSelection();
        }
    }
}
