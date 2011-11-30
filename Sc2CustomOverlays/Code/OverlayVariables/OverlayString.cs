using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Controls;

namespace Sc2CustomOverlays
{
    public class OverlayString : OverlayVariable
    {

        public override string Value
        {
            get
            {
                if (value == null)
                    return defaultValue;

                return value;
            }
        }

        protected string defaultValue = "";
        protected string _value = null;
        protected string value
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

        private TextBox MyTextbox = null;



        public override void FromXML(XmlNode vNode)
        {
            base.FromXML(vNode);

            foreach (XmlAttribute vNodeAttrib in vNode.Attributes)
            {
                switch (vNodeAttrib.LocalName)
                {
                    case "default":
                        defaultValue = vNodeAttrib.Value;
                        break;
                    case "value":
                        value = vNodeAttrib.Value;
                        break;
                }
            }
        }

        public override OverlayControlsContainer GetElements()
        {
            OverlayControlsContainer occ = base.GetElements();

            MyTextbox = new TextBox() { Text = Value, Width = 150 };

            occ.modifier = MyTextbox;
            
            occ.save.AddHandler(Button.ClickEvent, new RoutedEventHandler(SaveUpdateHandler));
            occ.reset.AddHandler(Button.ClickEvent, new RoutedEventHandler(ClearUpdateHandler));

            return occ;
        }

        private void SaveUpdateHandler(object sender, RoutedEventArgs rea)
        {
            if (MyTextbox != null)
            {
                value = MyTextbox.Text;
            }
        }

        private void ClearUpdateHandler(object sender, RoutedEventArgs rea)
        {
            value = null;

            if (MyTextbox != null)
                MyTextbox.Text = Value;
        }

        
    }
}
