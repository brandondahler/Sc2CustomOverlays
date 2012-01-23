// -----------------------------------------------------------------------
// <copyright file="OverlayStringViewModel.cs" company="">
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
    using System.ComponentModel;
    using Sc2CustomOverlays.Models.MVVMHelpers.Commands;
    using System.Windows.Input;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class OverlayStringViewModel : OverlayVariableBaseModel
    {
        // Commands
        public ICommand Save { get { return new DelegateCommand(View_Save); } }
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
            private string _defaultValue = "";
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

        #region TextBoxValue
            private string _textBoxValue = "";
            public string TextBoxValue 
            { 
                get { return _textBoxValue; }
                set
                {
                    _textBoxValue = value;
                    RaisePropertyChanged("TextBoxValue");
                }
            }
        #endregion

        public OverlayStringViewModel()
        {
            TextBoxValue = StringValue;
        }

        public override void FromXML(XmlNode vNode)
        {
            base.FromXML(vNode);

            foreach (XmlAttribute vNodeAttrib in vNode.Attributes)
            {
                switch (vNodeAttrib.LocalName)
                {
                    case "default":
                        DefaultValue = vNodeAttrib.Value;
                        break;
                    case "value":
                        Value = vNodeAttrib.Value;
                        break;
                }
            }
        }

        protected override void RaiseUpdated()
        {
            base.RaiseUpdated();

            TextBoxValue = StringValue;
        }

        private void View_Save()
        {
            Value = TextBoxValue;
        }

        private void View_Clear()
        {
            Value = null;
        }

    }
}
