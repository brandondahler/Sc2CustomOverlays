using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sc2CustomOverlays.Code.Exceptions
{
    public enum VariableParseFailure
    {
        InvalidXML = 0,
        DuplicateVariable,
        NullVariable
    }

    public class VariableParseException : Exception
    {
        public VariableParseException(VariableParseFailure reason) : base("Unable to process variables: " + GetMessage(reason))
        {

        }

        private static string GetMessage(VariableParseFailure reason)
        {
            switch (reason)
            {
                case VariableParseFailure.InvalidXML:
                    return "Invalid XML supplied.";

                case VariableParseFailure.DuplicateVariable:
                    return "Two or more variables with same name.";

                case VariableParseFailure.NullVariable:
                    return "One or more variables with no name.";
            }

            return "Unknown parse error.";
        }

    }
}
