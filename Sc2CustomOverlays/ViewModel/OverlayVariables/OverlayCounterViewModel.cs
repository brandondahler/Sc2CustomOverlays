// -----------------------------------------------------------------------
// <copyright file="OverlayCounterViewModel.cs" company="">
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
    using Sc2CustomOverlays.Models.Exceptions;
    using System.Windows.Input;
    using Sc2CustomOverlays.Models.MVVMHelpers.Commands;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class OverlayCounterViewModel : OverlayVariableBaseModel
    {
        // Commands
        public ICommand Add { get { return new DelegateCommand(View_Add); } }
        public ICommand Subtract { get { return new DelegateCommand(View_Subtract); } }
        public ICommand Clear { get { return new DelegateCommand(View_Clear); } }

        // Properties
        #region StringValue
            public override string StringValue
            {
                get
                {
                    if (!Value.HasValue)
                        return DefaultValue.ToString();

                    return Value.ToString();
                }
                set 
                {
                    if (value == null)
                        Value = null;
                    else
                        Value = int.Parse(value); 
                }
            }
        #endregion

        #region Value
            private int? _value = null;
            protected int? Value
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
            private int _defaultValue = 0;
            protected int DefaultValue
            {
                get { return _defaultValue; }
                set
                {
                    _defaultValue = value;
                    RaiseUpdated();
                }
            }
        #endregion

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
                            DefaultValue = int.Parse(vNodeAttrib.Value);
                            break;
                        case "value":
                            Value = int.Parse(vNodeAttrib.Value);
                            break;
                    }
                } catch (FormatException) {
                    throw new InvalidXMLValueException("OverlayCounter", vNodeAttrib.Value, InvalidXMLValueException.Reason.FormatIncorrect);
                } catch (OverflowException) {
                    throw new InvalidXMLValueException("OverlayCounter", vNodeAttrib.Value, InvalidXMLValueException.Reason.Overflow);
                }
            }
        }

        private void View_Add()
        {
            if (!Value.HasValue)
                Value = DefaultValue + 1;
            else
                Value += 1;
        }

        private void View_Subtract()
        {
            if (!Value.HasValue)
                Value = DefaultValue - 1;
            else
                Value -= 1;
        }

        private void View_Clear()
        {
            Value = null;
        }
    }
}
