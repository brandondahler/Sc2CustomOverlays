using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sc2CustomOverlays.Code.Networking.Control.Commands
{

    public class CommandException : Exception
    {
        public enum Reason
        {
            MissingParameter = 0,
            InvalidParameterType
        }

        public CommandException(string commandName, bool send, Reason reason) 
            : base((send ? "Send " : "Receive") + " :: " + commandName + GetMessage(reason)) { }

        protected static string GetMessage(Reason reason)
        {

            switch (reason)
            {
                case Reason.MissingParameter:
                    return " is missing a required parameter.";

                case Reason.InvalidParameterType:
                    return " has a parameter of an invalid type.";

            }

            return " failed for an unknown reason.";
        }
    }
}
