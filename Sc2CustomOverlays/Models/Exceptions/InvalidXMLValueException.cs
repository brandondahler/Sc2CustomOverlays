using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sc2CustomOverlays.Models.Exceptions
{

    class InvalidXMLValueException : Exception
    {
        public enum Reason
        {
            FormatIncorrect = 0,
            Overflow,
            NotSpecified,
            InvalidValue
        }

        public InvalidXMLValueException(string xmlNode, string xmlAttribute, Reason reason) 
            : base(xmlNode + " :: " + xmlAttribute + GetMessage(reason)) { }

        protected static string GetMessage(Reason reason)
        {

            switch (reason)
            {
                case Reason.FormatIncorrect:
                    return " is in an invalid format.";

                case Reason.Overflow:
                    return " is too large to be parsed.";

                case Reason.NotSpecified:
                    return " was not assigned a value.";

                case Reason.InvalidValue:
                    return " has an invalid value.";
            }

            return " is invalid for an unknown reason.";
        }
    }
}
