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
using System.Windows.Shapes;

namespace Sc2CustomOverlays
{
    /// <summary>
    /// Interaction logic for Overlay.xaml
    /// </summary>
    public partial class OverlayWindow : Window
    {
        public bool AllowClose = false;
        public bool AllowDrag = true;

        public Overlay MyOverlay = null;

        public OverlayWindow(Overlay o)
        {
            InitializeComponent();

            MyOverlay = o;

            ReloadOverlayControls();
        }

        public void ReloadOverlayControls()
        {
            overlayGrid.Children.Clear();

            foreach (FrameworkElement fe in MyOverlay.GetOverlayControls())
                overlayGrid.Children.Add(fe);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (AllowDrag)
                DragMove();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!AllowClose)
                e.Cancel = true;
        }

        
    }
}
