using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sc2CustomOverlays.Code.Exceptions
{
    
    class OverlayLoadingException : Exception
    {
        public enum Reason
        {
            InvalidXMLFormat = 0,
            VariableProcessing,
            OverlayCreation
        }

        public OverlayLoadingException(Reason reason)
            : base("Unable to open OverlaySettings: " + GetMessage(reason))
        {

        }

        public static string GetMessage(Reason reason)
        {
            switch (reason)
            {
                case Reason.InvalidXMLFormat:
                    return "XML does not appear to be a valid OverlaySetting format.";

                case Reason.VariableProcessing:
                    return "Variable processing failed.";

                case Reason.OverlayCreation:
                    return "Overlay creation failed.";
            }

            return "Unknown reason.";
        }
    }
}
