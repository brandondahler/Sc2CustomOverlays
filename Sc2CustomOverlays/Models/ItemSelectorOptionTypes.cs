// -----------------------------------------------------------------------
// <copyright file="ItemSelectorOptionTypes.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Sc2CustomOverlays.Models.ItemSelectorOptionTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Media;
    using Sc2CustomOverlays.Models.MVVMHelpers;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
        public class ItemSelectorOption : ObservableModel
        {
            #region IsChecked
                private bool _isChecked = false;
                public bool IsChecked 
                { 
                    get { return _isChecked; }
                    set
                    {
                        _isChecked = value;
                        RaisePropertyChanged("IsChecked");
                    }
                }
            #endregion
            public string Value { get; set; }
            public string Alt { get; set; }
        }
        
        public class ItemSelectorLabel : ItemSelectorOption
        {
            public string Label { get; set; }
        }
        
        public class ItemSelectorImage : ItemSelectorOption
        {
            public string Location { get; set; }
        }
        
        public class ItemSelectorColor : ItemSelectorOption
        {
            public Brush Fill { get; set; }
        }
}
