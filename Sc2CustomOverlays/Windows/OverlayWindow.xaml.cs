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
using Sc2CustomOverlays.Models;
using System.Xml;
using Sc2CustomOverlays.Models.Exceptions;
using System.IO;
using System.ComponentModel;
using Sc2CustomOverlays.ViewModel.OverlayItems;
using System.Collections.ObjectModel;
using Sc2CustomOverlays.ViewModel;

namespace Sc2CustomOverlays.Windows
{
    /// <summary>
    /// Interaction logic for Overlay.xaml
    /// </summary>
    public partial class OverlayWindow : Window
    {
        public bool AllowClose = false;
        public bool AllowDrag = false;

        public OverlayWindow()
        {
            InitializeComponent();
            ((OverlayWindowModel) DataContext).PropertyChanged += new PropertyChangedEventHandler(OverlayWindowModel_PropertyChanged);
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ((OverlayWindowModel) DataContext).ActualHeight = ActualHeight;
            ((OverlayWindowModel) DataContext).ActualWidth = ActualWidth;
        }

        // Top and Left must be manually updated, Binding doesn't work on these
        private void OverlayWindowModel_PropertyChanged(object sender, PropertyChangedEventArgs pcea)
        {
            if (pcea.PropertyName == "Top")
                Top = ((OverlayWindowModel) DataContext).Top;

            if (pcea.PropertyName == "Left")
                Left = ((OverlayWindowModel) DataContext).Left;
        }

    }
}
