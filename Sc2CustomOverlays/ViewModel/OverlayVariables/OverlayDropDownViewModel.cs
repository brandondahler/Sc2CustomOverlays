// -----------------------------------------------------------------------
// <copyright file="OverlayDropDownViewModel.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Sc2CustomOverlays.ViewModel.OverlayVariables
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Collections.ObjectModel;
    using System.Xml;
    using Sc2CustomOverlays.Models.Exceptions;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class OverlayDropDownViewModel : OverlayVariableBaseModel
    {
        #region StringValue
            public override string StringValue
            {
                get
                {
                    if (Value == null)
                        return DefaultValue;

                    return Value;
                }
                set { Value = value; }
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

        #region DropDownOptions
            private ObservableCollection<DropDownOption> _dropDownOptions = new ObservableCollection<DropDownOption>();
            public ObservableCollection<DropDownOption> DropDownOptions { get { return _dropDownOptions; } }
        #endregion
        #region DropDownValue
            private string _dropDownValue;
            public string DropDownValue
            {
                get { return _dropDownValue; }
                set
                {
                    _dropDownValue = value;
                    RaisePropertyChanged("DropDownValue");
                }
            }
        #endregion

        public struct DropDownOption
        {
            public string name;
            public string value;
        }

        public OverlayDropDownViewModel()
        {
            DropDownValue = StringValue;
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
                            DefaultValue = vNodeAttrib.Value;
                            break;
                    }
                } catch (FormatException) {
                    throw new InvalidXMLValueException("OverlayDropDown", vNodeAttrib.Value, InvalidXMLValueException.Reason.FormatIncorrect);
                } catch (OverflowException) {
                    throw new InvalidXMLValueException("OverlayDropDown", vNodeAttrib.Value, InvalidXMLValueException.Reason.Overflow);
                }
            }

            XmlNodeList oNodes = vNode.SelectNodes("Option");
            foreach (XmlNode oNode in oNodes)
            {
                string lbl = null;
                string val = null;
                
                foreach (XmlAttribute xAttrib in oNode.Attributes)
                {
                    switch (xAttrib.LocalName)
                    {
                        case "label":
                            lbl = xAttrib.Value;
                            break;

                        case "value":
                            val = xAttrib.Value;
                            break;
                    }
                }

                if (lbl != null && val == null)
                    val = lbl;

                if (lbl == null && val != null)
                    lbl = val;

                if (lbl != null && val != null)
                    DropDownOptions.Add(new DropDownOption() { name = lbl, value = val });
            }


        }

        protected override void RaiseUpdated()
        {
            base.RaiseUpdated();
            DropDownValue = StringValue;
        }

        private void View_Save()
        {    
            Value = DropDownValue;
        }

        private void View_Clear()
        {
            Value = null;
        }
    }
}
