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

namespace Sc2CustomOverlays.Code.OverlayVariables
{
    /// <summary>
    /// Interaction logic for OverlayString.xaml
    /// </summary>
    public partial class OverlayString : OverlayVariable
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

        protected string defaultValue = "";

        public OverlayString()
        {
            InitializeComponent();
        }

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
                        currentValue = vNodeAttrib.Value;
                        break;
                }
            }
        }

        public override OverlayControlsContainer GetElements()
        {
            OverlayControlsContainer occ = base.GetElements();

            ValueTextBox.Text = Value;

            occ.save.AddHandler(Button.ClickEvent, new RoutedEventHandler(SaveUpdateHandler));
            occ.reset.AddHandler(Button.ClickEvent, new RoutedEventHandler(ClearUpdateHandler));

            return occ;
        }

        private void SaveUpdateHandler(object sender, RoutedEventArgs rea)
        {
            currentValue = ValueTextBox.Text;
        }

        private void ClearUpdateHandler(object sender, RoutedEventArgs rea)
        {
            currentValue = null;
            ValueTextBox.Text = Value;
        }

    }
}
