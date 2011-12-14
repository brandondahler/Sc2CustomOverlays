using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sc2CustomOverlays.Code.Exceptions
{
    public enum OverlayCreationFailure
    {
        InvalidXML = 0
    }

    public class OverlayCreationException : Exception
    {

        public OverlayCreationException(OverlayCreationFailure reason) : base("Unable to create overlay: " + GetMessage(reason))
        {

        }


        public static string GetMessage(OverlayCreationFailure reason)
        {
            switch (reason)
            {
                case OverlayCreationFailure.InvalidXML:
                    return "Invalid XML supplied.";
            }

            return "Unknown creation error.";
        }
    }
}
