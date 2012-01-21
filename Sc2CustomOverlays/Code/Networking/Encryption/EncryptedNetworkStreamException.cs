using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sc2CustomOverlays.Code.Networking.Encryption
{
    public class EncryptedNetworkStreamException : Exception
    {
        public enum Reason
        {
            NetworkError = 0,
            ReadTimeout,
            InvalidAuthentication
        }

        private Reason _exceptionReason;
        public Reason ExceptionReason { get { return _exceptionReason; } }

        public EncryptedNetworkStreamException(Reason reason) : base("EncryptedNetworkStream :: " + GetMessage(reason))
        {
            _exceptionReason = reason;
        }

        private static string GetMessage(Reason reason)
        {
            switch (reason)
            {
                case Reason.NetworkError:
                    return "Network encountered an error.";
                   
                case Reason.ReadTimeout:
                    return "An authentication read timed out.";

                case Reason.InvalidAuthentication:
                    return "Invalid authentication received.";
            }

            return "Unknown error.";
        }

    }
}
