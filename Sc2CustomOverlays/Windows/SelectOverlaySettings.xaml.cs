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
using Sc2CustomOverlays.Code;
using System.ComponentModel;

namespace Sc2CustomOverlays.Windows
{
    /// <summary>
    /// Interaction logic for SelectOverlaySettings.xaml
    /// </summary>
    /// 

    public delegate void SettingSelectedHandler(AvailableOverlaySetting selectedSetting);

    public partial class SelectOverlaySettings : Window, INotifyPropertyChanged
    {
        public event SettingSelectedHandler SettingSelected;
        public event PropertyChangedEventHandler PropertyChanged;

        private List<AvailableOverlaySetting> _availableSettings = new List<AvailableOverlaySetting>();
        public List<AvailableOverlaySetting> AvailableSettings 
        {
            get { return _availableSettings; }
            set
            {
                _availableSettings = value;
                RaisePropertyChanged("AvailableSettings");
            }
        }

        private bool _localOnly = false;
        public bool LocalOnly
        {
            get { return _localOnly; }
            set
            {
                _localOnly = value;

                if (_localOnly)
                    dgSettings.Columns[1].Visibility = System.Windows.Visibility.Collapsed;
                else
                    dgSettings.Columns[1].Visibility = System.Windows.Visibility.Visible;

            }
        }

        public SelectOverlaySettings()
        {
            InitializeComponent();

        }

        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void btnUseSelected_Click(object sender, RoutedEventArgs e)
        {
            if (dgSettings.SelectedItem != null)
            {
                if (SettingSelected != null)
                    SettingSelected((AvailableOverlaySetting)dgSettings.SelectedItem);

                Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
