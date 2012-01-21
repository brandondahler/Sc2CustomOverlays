using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sc2CustomOverlays.Code.Exceptions
{
    

    class VariableParseException : Exception
    {
        public enum Reason
        {
            InvalidXML = 0,
            DuplicateVariable,
            NullVariable
        }

        public VariableParseException(Reason reason)
            : base("Unable to process variables: " + GetMessage(reason))
        {

        }

        private static string GetMessage(Reason reason)
        {
            switch (reason)
            {
                case Reason.InvalidXML:
                    return "Invalid XML supplied.";

                case Reason.DuplicateVariable:
                    return "Two or more variables with same name.";

                case Reason.NullVariable:
                    return "One or more variables with no name.";
            }

            return "Unknown parse error.";
        }

    }
}
