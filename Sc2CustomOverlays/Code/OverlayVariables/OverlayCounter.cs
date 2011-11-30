using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Controls;


namespace Sc2CustomOverlays
{
    public class OverlayCounter : OverlayVariable
    {

        public override string Value
        {
            get
            {
                if (value.HasValue)
                    return value.Value.ToString();

                return defaultValue.ToString();
            }
        }
        
        private int defaultValue = 0;
        private int? _value = null;

        private int? value
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

        private Label MyLabel = null;



        public override void FromXML(XmlNode vNode)
        {
            base.FromXML(vNode);

            foreach (XmlAttribute vNodeAttrib in vNode.Attributes)
            {
                switch (vNodeAttrib.LocalName)
                {
                    case "default":
                        defaultValue = int.Parse(vNodeAttrib.Value);
                        break;
                    case "value":
                        value = int.Parse(vNodeAttrib.Value);
                        break;
                }
            }
        }

        public override OverlayControlsContainer GetElements()
        {
            OverlayControlsContainer occ = base.GetElements();

            StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal };

            Button sub = new Button() { Content = "-", Margin = new Thickness(0, 0, 5, 0), Padding = new Thickness(8, 0, 8, 0) };
            sub.AddHandler(Button.ClickEvent, new RoutedEventHandler(SubtractCountHandler));

            MyLabel = new Label() { Content = Value, Margin = new Thickness(0, 0, 3, 0), MinWidth = 30, HorizontalContentAlignment = HorizontalAlignment.Center };

            Button add = new Button() { Content = "+", Margin = new Thickness(0, 0, 3, 0), Padding = new Thickness(8, 0, 8, 0) };
            add.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddCountHandler));

            sp.Children.Add(sub);
            sp.Children.Add(MyLabel);
            sp.Children.Add(add);

            occ.modifier = sp;
            occ.save = null;

            occ.reset.AddHandler(Button.ClickEvent, new RoutedEventHandler(ClearCountHandler));

            return occ;
        }

        private void SubtractCountHandler(object sender, RoutedEventArgs rea)
        {
            if (!value.HasValue)
                value = defaultValue;

            value -= 1;

            if (MyLabel != null)
                MyLabel.Content = Value;
        }

        private void AddCountHandler(object sender, RoutedEventArgs rea)
        {
            if (!value.HasValue)
                value = defaultValue;

            value += 1;

            if (MyLabel != null)
                MyLabel.Content = Value;
        }

        private void ClearCountHandler(object sender, RoutedEventArgs rea)
        {
            value = null;

            if (MyLabel != null)
                MyLabel.Content = Value;
        }
    }
}
