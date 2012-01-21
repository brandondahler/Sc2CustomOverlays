using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sc2CustomOverlays.Code.Exceptions
{
    

    class OverlayCreationException : Exception
    {
        public enum Reason
        {
            InvalidXML = 0
        }

        public OverlayCreationException(Reason reason)
            : base("Unable to create overlay: " + GetMessage(reason))
        {

        }


        public static string GetMessage(Reason reason)
        {
            switch (reason)
            {
                case Reason.InvalidXML:
                    return "Invalid XML supplied.";
            }

            return "Unknown creation error.";
        }
    }
}
