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

    public partial class OverlayCounter : OverlayVariable
    {
        #region Properties

            public override string Value
            {
                get
                {
                    if (currentValue.HasValue)
                        return currentValue.Value.ToString();

                    return defaultValue.ToString();
                }
            }

            private int? _currentValue = null;
            protected int? currentValue
            {
                get { return _currentValue; }
                set
                {
                    _currentValue = value;
                    RaiseUpdated();
                }
            }
            private int defaultValue = 0;

        #endregion

        public OverlayCounter()
        {
            InitializeComponent();
            ValueLabel.Content = Value;
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
                            defaultValue = int.Parse(vNodeAttrib.Value);
                            break;
                        case "value":
                            currentValue = int.Parse(vNodeAttrib.Value);
                            break;
                    }
                } catch (FormatException) {
                    throw new InvalidXMLValueException("OverlayCounter", vNodeAttrib.Value, InvalidValueReason.FormatIncorrect);
                } catch (OverflowException) {
                    throw new InvalidXMLValueException("OverlayCounter", vNodeAttrib.Value, InvalidValueReason.Overflow);
                }
            }
        }

        public override OverlayControlsContainer GetElements()
        {
            OverlayControlsContainer occ = base.GetElements();
            
            occ.save = null;
            occ.reset.AddHandler(Button.ClickEvent, new RoutedEventHandler(ClearCountHandler));

            return occ;
        }


         private void SubtractCountHandler(object sender, RoutedEventArgs rea)
        {
            if (!currentValue.HasValue)
                currentValue = defaultValue;

            currentValue -= 1;
        }

        private void AddCountHandler(object sender, RoutedEventArgs rea)
        {
            if (!currentValue.HasValue)
                currentValue = defaultValue;

            currentValue += 1;
        }

        private void ClearCountHandler(object sender, RoutedEventArgs rea)
        {
            currentValue = null;
        }

        private void OverlayVariable_Updated()
        {
            ValueLabel.Content = Value;
        }
    }

}
