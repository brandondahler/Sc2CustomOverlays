// -----------------------------------------------------------------------
// <copyright file="OverlayControlViewModel.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Sc2CustomOverlays.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using System.Windows;
    using System.Windows.Input;
    using System.ComponentModel;
    
    using Sc2CustomOverlays.Models.MVVMHelpers;
    using Sc2CustomOverlays.Models.MVVMHelpers.Commands;

    using Sc2CustomOverlays.Models;
    using Sc2CustomOverlays.ViewModel.OverlayVariables;
    
    

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class OverlayControlViewModel : ObservableModel
    {

        public IEnumerable<OverlayVariableBaseModel> OverlayVariableControls { get {  return OverlaySettings.Instance.OverlayControls; } }
        public ICommand ShowOverlay { get { return new DelegateCommand(View_ShowOverlay); } }

        public OverlayControlViewModel()
        {
            OverlaySettings.Instance.PropertyChanged += new PropertyChangedEventHandler(OverlaySettings_PropertyChanged);
        }

        public void OverlaySettings_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "OverlayControls")
                RaisePropertyChanged("OverlayVariableControls");
        }

        private void View_ShowOverlay()
        {
            OverlaySettings.Instance.ToggleOverlayVisibility();
        }
    }
}
