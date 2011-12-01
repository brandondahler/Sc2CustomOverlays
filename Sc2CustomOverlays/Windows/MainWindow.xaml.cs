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
using Microsoft.Win32;
using System.Windows.Controls.Primitives;

namespace Sc2CustomOverlays
{
    public partial class MainWindow : Window
    {
        private OverlaySettings overlaySettings = null;
        private List<OverlayWindow> overlayWindows = new List<OverlayWindow>();

        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnShowOverlays_Click(object sender, RoutedEventArgs e)
        {

            if (overlayWindows.Count() > 0)
            {
                CloseOverlayWindows();
            } else {

                foreach (Overlay o in overlaySettings.GetOverlays())
                {
                    OverlayWindow w = new OverlayWindow(o);
                    w.Show();

                    overlayWindows.Add(w);
                }
            }

            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseOverlayWindows();
        }

        private void CloseOverlayWindows()
        {
            foreach (OverlayWindow w in overlayWindows)
            {
                w.AllowClose = true;
                w.Close();
            }

            overlayWindows.Clear();
        }

        private void menuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = "*.xml";
            ofd.FileName = "Default.xml";
            ofd.Filter = "Overlays (*.xml)|*.xml";
            ofd.InitialDirectory = Environment.CurrentDirectory + "\\Overlays";
            ofd.Multiselect = false;
            ofd.ValidateNames = false;

            bool? selected = ofd.ShowDialog();
            if (selected != null && selected.Value)
            {
                CloseOverlayWindows();

                string overlayFile = GetRelativePath(ofd.FileName);
                overlaySettings = new OverlaySettings(overlayFile);
                LoadControls();
            }
        }

        private void menuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private string GetRelativePath(string fileLocation)
        {
            if (fileLocation.IndexOf(Environment.CurrentDirectory + "\\", StringComparison.OrdinalIgnoreCase) >= 0)
                fileLocation = fileLocation.Substring((Environment.CurrentDirectory + "\\").Length);

            return fileLocation;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                overlaySettings = new OverlaySettings("Overlays\\Starboard\\Starboard.xml");
                LoadControls();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
            
        }

        private void LoadControls()
        {
            OverlayControls.Children.Clear();
            OverlayControls.RowDefinitions.Clear();

            
            Dictionary<string, GroupBox> variableGroupBoxes = new Dictionary<string, GroupBox>();

            foreach (KeyValuePair<string, string> variableGroup in overlaySettings.GetVariableGroups())
            {
                GroupBox variableGroupBox = new GroupBox();
                Grid variableGroupBoxGrid = new Grid();

                variableGroupBoxGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                variableGroupBoxGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                variableGroupBoxGrid.ColumnDefinitions.Add(new ColumnDefinition() { });
                variableGroupBoxGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                variableGroupBoxGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                variableGroupBox.Content = variableGroupBoxGrid;

                variableGroupBox.Header = variableGroup.Value;
                variableGroupBox.SetValue(Grid.ColumnSpanProperty, 5);
                variableGroupBox.SetValue(Grid.RowProperty, OverlayControls.RowDefinitions.Count);
                

                OverlayControls.RowDefinitions.Add(new RowDefinition());
                OverlayControls.Children.Add(variableGroupBox);

                variableGroupBoxes.Add(variableGroup.Key, variableGroupBox);
            }

            foreach (OverlayVariable ov in overlaySettings.GetVariables())
            {
                Grid container = OverlayControls;

                if (ov.Group != null && variableGroupBoxes.ContainsKey(ov.Group))
                    container = (Grid) variableGroupBoxes[ov.Group].Content;

                OverlayControlsContainer occ = ov.GetElements();
                int rowNum = container.RowDefinitions.Count;

                container.RowDefinitions.Add(new RowDefinition());

                AddOverlayControl(container, occ.label, 0, rowNum);
                AddOverlayControl(container, occ.modifier, 1, rowNum);
                AddOverlayControl(container, occ.save, 3, rowNum);
                AddOverlayControl(container, occ.reset, 4, rowNum);

            }
        }

        private void AddOverlayControl(Grid container, UIElement control, int column, int row)
        {
            if (control == null)
                return;

            control.SetValue(Grid.ColumnProperty, column);
            control.SetValue(Grid.RowProperty, row);

            control.SetValue(Control.MarginProperty, new Thickness(0, 0, 15, 5));

            container.Children.Add(control);
        }


    }
}



