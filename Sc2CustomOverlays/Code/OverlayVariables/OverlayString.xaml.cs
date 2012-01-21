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

        private string _defaultValue = "";
        protected string defaultValue
        {
            get { return _defaultValue; }
            set
            {
                _defaultValue = value;
                RaiseUpdated();
            }
        }

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
            currentValue = ValueTextBox.Text;
        }

        private void ClearUpdateHandler(object sender, RoutedEventArgs rea)
        {
            currentValue = null;
        }

    }
}
