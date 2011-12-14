using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sc2CustomOverlays.Code.Exceptions
{
    public enum InvalidValueReason
    {
        FormatIncorrect = 0,
        Overflow,
        NotSpecified,
        InvalidValue
    }

    public class InvalidXMLValueException : Exception
    {
        public InvalidXMLValueException(string xmlNode, string xmlAttribute, InvalidValueReason reason ) : base(xmlNode + " :: " + xmlAttribute + GetMessage(reason)) { }
        
        protected static string GetMessage(InvalidValueReason reason)
        {

            switch (reason)
            {
                case InvalidValueReason.FormatIncorrect:
                    return " is in an invalid format.";

                case InvalidValueReason.Overflow:
                    return " is too large to be parsed.";

                case InvalidValueReason.NotSpecified:
                    return " was not assigned a value.";

                case InvalidValueReason.InvalidValue:
                    return " has an invalid value.";
            }

            return " is invalid for an unknown reason.";
        }
    }
}
