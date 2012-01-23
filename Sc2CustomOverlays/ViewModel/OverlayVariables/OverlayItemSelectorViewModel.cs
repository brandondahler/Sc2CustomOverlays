// -----------------------------------------------------------------------
// <copyright file="OverlayItemSelectorViewModel.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Sc2CustomOverlays.ViewModel.OverlayVariables
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Collections.ObjectModel;
    using System.Windows.Media;
    using Sc2CustomOverlays.Models.Exceptions;
    using System.IO;
    using Sc2CustomOverlays.Models;
    using Sc2CustomOverlays.Models.ItemSelectorOptionTypes;
    using System.ComponentModel;
    using Sc2CustomOverlays.Models.MVVMHelpers.Commands;
    using System.Windows.Input;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class OverlayItemSelectorViewModel : OverlayVariableBaseModel
    {
        // Commands
        public ICommand Clear { get { return new DelegateCommand(View_Clear); } }

        // Properties
        #region StringValue
        public override string StringValue
        {
            get
            {
                if (Value == null)
                    return DefaultValue;

                return Value;
            }

            set
            {
                Value = value;   
            }
        }
        #endregion

        #region Value
            private string _value = null;
            protected string Value
            {
                get { return _value; }
                set
                {
                    _value = value;
                    RaiseUpdated();
                }
            }
        #endregion
        #region DefaultValue
            private string _defaultValue = null;
            protected string DefaultValue
            {
                get { return _defaultValue; }
                set
                {
                    _defaultValue = value;
                    RaiseUpdated();
                }
            }
        #endregion
        #region ItemHeight
            private double _itemHeight = double.NaN;
            public double ItemHeight 
            { 
                get { return _itemHeight; } 
                set
                {
                    _itemHeight = value;
                    RaisePropertyChanged("ItemHeight");
                }
            }
        #endregion
        #region ItemWidth
            private double _itemWidth = double.NaN;
            public double ItemWidth
            { 
                get { return _itemWidth; } 
                set
                {
                    _itemWidth = value;
                    RaisePropertyChanged("ItemWidth");
                }
            }
        #endregion
  
        #region SelectionOptions
            private ObservableCollection<ItemSelectorOption> _selectionOptions = new ObservableCollection<ItemSelectorOption>();
            public ObservableCollection<ItemSelectorOption> SelectionOptions { get { return _selectionOptions; } }
        #endregion
        #region SelectorName
            private static int SelectorId = 0;
            private string _selectorName = null;
            public string SelectorName 
            {
                get 
                {
                    if (_selectorName == null)
                    {
                        _selectorName = (SelectorId++).ToString();
                        RaisePropertyChanged("SelectorName");
                    }

                    return _selectorName;
                }
            }
        #endregion


        #region Columns
            protected int _columns = 0;
            public int Columns
            { 
                get 
                {
                    if (SelectionOptions.Count < _columns)
                        return SelectionOptions.Count;

                    return _columns;
                }
                set
                {
                    _columns = value;
                    RaisePropertyChanged("Columns");
                }
            }
        #endregion


        public override void FromXML(XmlNode vNode)
        {
            base.FromXML(vNode);

            SelectionOptions.Clear();
            XmlNodeList oNodes = vNode.ChildNodes;

            foreach (XmlNode oNode in oNodes)
            {
                ItemSelectorOption option = null;
                try
                {
                    switch (oNode.LocalName)
                    {
                        case "Text":
                            string labelText = oNode.Attributes.GetNamedItem("text").Value;
                            
                            option = new ItemSelectorLabel { Label = labelText };
                            break;
                        case "Image":
                            string imageLocation = Path.Combine(OverlaySettings.Instance.Location.Directory.FullName,
                                                                oNode.Attributes.GetNamedItem("image").Value);
                            
                            option = new ItemSelectorImage { Location = imageLocation };
                            break;
                        case "Color":
                            string colorValue = oNode.Attributes.GetNamedItem("color").Value;
                            if (ColorConverter.ConvertFromString(colorValue) == null)
                                throw new InvalidXMLValueException("OverlayItemSelector:Option", "color", InvalidXMLValueException.Reason.InvalidValue);

                            option = new ItemSelectorColor { Fill = new SolidColorBrush((Color) ColorConverter.ConvertFromString(colorValue)) };
                            break;
                    }
                } catch (Exception) {
                    throw new InvalidXMLValueException("OverlayItemSelector:Option", oNode.LocalName, InvalidXMLValueException.Reason.NotSpecified);
                }

                if (option == null)
                    throw new InvalidXMLValueException("OverlayItemSelector:Option", oNode.LocalName, InvalidXMLValueException.Reason.InvalidValue);
                

                foreach (XmlAttribute xAttrib in oNode.Attributes)
                {
                    switch (xAttrib.LocalName)
                    {
                        case "value":
                            option.Value = xAttrib.Value;
                            break;

                        case "alt":
                            option.Alt = xAttrib.Value;
                            break;
                    }
                }
                
                option.PropertyChanged += new PropertyChangedEventHandler(ItemSelectorOption_PropertyChanged);
                SelectionOptions.Add(option);
            }

            RaisePropertyChanged("Columns");
            
            foreach (XmlAttribute vNodeAttrib in vNode.Attributes)
            {
                try
                {
                    switch (vNodeAttrib.LocalName)
                    {
                        case "default":
                            DefaultValue = vNodeAttrib.Value;
                            break;
                        case "value":
                            Value = vNodeAttrib.Value;
                            break;
                        case "columns":
                            Columns = int.Parse(vNodeAttrib.Value);
                            break;
                        case "itemHeight":
                            ItemHeight = double.Parse(vNodeAttrib.Value);
                            break;
                        case "itemWidth":
                            ItemWidth = double.Parse(vNodeAttrib.Value);
                            break;
                    }
                } catch (FormatException) {
                    throw new InvalidXMLValueException("OverlayItemSelector", vNodeAttrib.Value, InvalidXMLValueException.Reason.FormatIncorrect);
                } catch (ArgumentNullException) {
                    throw new InvalidXMLValueException("OverlayItemSelector", vNodeAttrib.Value, InvalidXMLValueException.Reason.NotSpecified);
                } catch (OverflowException) {
                    throw new InvalidXMLValueException("OverlayItemSelector", vNodeAttrib.Value, InvalidXMLValueException.Reason.Overflow);
                }
            }

            if (DefaultValue == null && SelectionOptions.Count > 0)
                DefaultValue = SelectionOptions.First().Value;
        }

        protected override void RaiseUpdated()
        {
            base.RaiseUpdated();

            foreach (ItemSelectorOption iso in SelectionOptions)
            {
                if (iso.Value == StringValue && iso.IsChecked == false)
                {
                    iso.IsChecked = true;
                    break;
                }
            }
        }

        private void ItemSelectorOption_PropertyChanged(object sender, PropertyChangedEventArgs pcea)
        {
            if (pcea.PropertyName == "IsChecked")
            {
                ItemSelectorOption iso = (ItemSelectorOption) sender;
                if (iso.IsChecked == true)
                    Value = iso.Value;
            }
        }

        private void View_Clear()
        {
            Value = null;
        }

    }
}
