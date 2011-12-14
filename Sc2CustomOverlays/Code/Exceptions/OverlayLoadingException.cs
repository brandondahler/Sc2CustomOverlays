using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sc2CustomOverlays.Code.Exceptions
{
    public enum OverlayLoadingFailure
    {
        VariableProcessing = 0,
        OverlayCreation
    }

    public class OverlayLoadingException : Exception
    {
        public OverlayLoadingException(OverlayLoadingFailure reason) : base("Unable to open OverlaySettings: " + GetMessage(reason))
        {

        }

        public static string GetMessage(OverlayLoadingFailure reason)
        {
            switch (reason)
            {
                case OverlayLoadingFailure.VariableProcessing:
                    return "Variable processing failed.";

                case OverlayLoadingFailure.OverlayCreation:
                    return "Overlay creation failed.";
            }

            return "Unknown reason.";
        }
    }
}
